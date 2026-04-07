using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public class WeatherParticleSystem : MonoBehaviour
{
    struct Particle
    {
        public Vector3 position; // 粒子位置
        public Vector3 velocity; // 粒子速度
        public float life;       // 粒子生命周期
    }
    
    struct SystemArgs {
        uint particleCount; //存活粒子数量
        uint maxParticleCount; //容器最大粒子数量
    }
    
    public ComputeShader spawnCS;
    public ComputeShader simulationCS;
    
    private int spawnKernelId; //CS 主函数（kernel) id
    private int simulationKernelId; //CS 主函数（kernel) id
    
    private ComputeBuffer particleBuffer_0;
    private ComputeBuffer particleBuffer_1;
    private ComputeBuffer systemArgsBuffer;
    
    // private GraphicsBuffer renderArgsBuffer;
    private ComputeBuffer renderArgsBuffer;
    // RenderParams rp;
    
    
    
    [Header("Particle Settings")]
    [Range(0, 100)]
    public int spawnCount = 10; //每帧生成数量
    [Range(0, 4096)]
    public int capacity = 100; //容器最大粒子数
    public Vector3 gravity;
    
    [Header("Render Settings")]
    public Mesh renderMesh; //渲染粒子的 网格
    public Material renderMaterial; //渲染粒子的 材质
    private Bounds particleBounds; //渲染粒子的 AABB
    
    private int currentFrameIndex; //运行帧号
    
    
    void Start()
    {
        if (spawnCS == null || simulationCS == null)
        {
            Debug.LogError("WeatherParticleSystem: ComputeShader 未赋值!");
            return;
        }
        Initialize();
    }
    

    void Update()
    {
        // 确保初始化完成
        if (particleBuffer_0 == null || particleBuffer_1 == null)
            return;

        DispatchSpawner();
        DispatchSimulation();
        DrawCall();

        currentFrameIndex++;
    }
    
    void OnDestroy()
    {
        particleBuffer_0?.Release();
        particleBuffer_0 = null;
        particleBuffer_1?.Release();
        particleBuffer_1 = null;
        systemArgsBuffer?.Release();
        systemArgsBuffer = null;
        renderArgsBuffer?.Release();
        renderArgsBuffer = null;
    }

    void OnDisable()
    {
        // 确保在禁用时也释放资源
        particleBuffer_0?.Release();
        particleBuffer_0 = null;
        particleBuffer_1?.Release();
        particleBuffer_1 = null;
        systemArgsBuffer?.Release();
        systemArgsBuffer = null;
        renderArgsBuffer?.Release();
        renderArgsBuffer = null;
    }

    
    /// <summary>
    /// 调用粒子生成的CS
    /// </summary>
    private void DispatchSpawner()
    {
        // 按照当前帧选择buffer
        var buffer = GetBuffer(currentFrameIndex);
        ComputeBuffer.CopyCount(buffer, systemArgsBuffer, 0); // 获取到计数值到systemArgsBuffer字节偏移位置
        
        spawnCS.SetBuffer(spawnKernelId, "_Particles",  buffer);
        spawnCS.SetFloat("_timeLoaded", Time.timeSinceLevelLoad);
        
        // 执行CS
        spawnCS.Dispatch(spawnKernelId, 16, 1, 1);

    }

    /// <summary>
    /// 调用粒子模拟的CS
    /// </summary>
    private void DispatchSimulation()
    {
        // 输入Buffer
        var buffer = GetBuffer(currentFrameIndex);
        // 输出Buffer，存储的是当前帧模拟后，存活下来的粒子，作为下一帧【SpawnCS】的输入
        var aliveBuffer = GetBuffer(currentFrameIndex + 1);
        // 需要进行清理，否则会保留上一帧的数据，导致结果错误
        aliveBuffer.SetCounterValue(0);
        
        // 设置存活粒子数量，需要注意偏移字节
        ComputeBuffer.CopyCount(buffer, systemArgsBuffer, 0);
        
        // 绑定Buffer
        simulationCS.SetBuffer(simulationKernelId, "_Particles", buffer);
        simulationCS.SetBuffer(simulationKernelId, "_AliveParticles", aliveBuffer);
        simulationCS.SetFloat("_deltaTime", Time.deltaTime); // 从上一帧到当前帧的间隔
        
        // 执行
        simulationCS.Dispatch(simulationKernelId, 32, 1, 1);
        
    }
    
    /// <summary>
    /// 渲染粒子
    /// </summary>
    private void DrawCall()
    {
        // 获取到死亡剔除后的Buffer
        var aliveBuffer = GetBuffer(currentFrameIndex + 1);

        // 将计数传递给绘制Buffer中，确保绘制数量正确
        // 并且由于里面每个参数的类似都是unit，占据4字节，indexCountPerInstance在第二个，所以起始位置需要偏移4
        ComputeBuffer.CopyCount(aliveBuffer, renderArgsBuffer, 4);

        GraphicsBuffer.IndirectDrawIndexedArgs[] args = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        renderArgsBuffer.GetData(args);

        // Debug.Log($"[Draw] aliveBuffer: {aliveBuffer != null}, instanceCount: {args[0].instanceCount}, currentFrameIndex: {currentFrameIndex}");

        renderMaterial.SetBuffer("_ParticleBuffer", aliveBuffer);
        particleBounds =  new Bounds(transform.position, Vector3.one * 1000);
        Graphics.DrawMeshInstancedIndirect(renderMesh, 0, renderMaterial, particleBounds, renderArgsBuffer);
        

        // 提交渲染命令
        // Graphics.RenderMeshIndirect(rp, renderMesh, renderArgsBuffer);
    }
    
    /// <summary>
    /// 根据帧号返回对应的particle buffer
    /// </summary>
    /// <param name="frameIndex"></param>
    /// <returns></returns>
    private ComputeBuffer GetBuffer(int frameIndex)
    {
        return frameIndex % 2 == 0 ? particleBuffer_0 : particleBuffer_1;
    }
    
    void Initialize()
    {
        // 使用Append类型的Buffer
        particleBuffer_0 = new ComputeBuffer(capacity, Marshal.SizeOf<Particle>(), ComputeBufferType.Append);
        particleBuffer_0.SetCounterValue(0);

        particleBuffer_1 = new ComputeBuffer(capacity, Marshal.SizeOf<Particle>(), ComputeBufferType.Append);
        particleBuffer_1.SetCounterValue(0);

        // 用于GPU Instance
        systemArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 2, ComputeBufferType.IndirectArguments);
        systemArgsBuffer.SetData(new uint[]{0, (uint)capacity});


        spawnKernelId = spawnCS.FindKernel("CSMain");
        SetupSpawn();

        simulationKernelId = simulationCS.FindKernel("CSMain");
        SetupSimulation();

        SetupRenderData();
        
    }

    void SetupSpawn()
    {
        spawnCS.SetInt("_spawnCount", spawnCount);
        spawnCS.SetBuffer(spawnKernelId,"_SystemArgs", systemArgsBuffer);
    }
    
    private void SetupSimulation()
    {
        simulationCS.SetVector("_gravity", gravity);
        simulationCS.SetBuffer(simulationKernelId, "_SystemArgs", systemArgsBuffer);
    }

    private void SetupRenderData()
    {
        if (renderMesh == null || renderMaterial == null)
        {
            Debug.LogError("WeatherParticleSystem: renderMesh 或 renderMaterial 未赋值!");
            return;
        }

        // // 初始化 Bounds（使用一个足够大的范围覆盖所有粒子可能出现的区域）
        // particleBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        // rp.worldBounds = particleBounds;
        // rp.material = renderMaterial;
        //
        // // 创建 GraphicsBuffer（用于 Indirect Draw Arguments）
        // // 每个 args 结构包含 5 个 uint：indexCountPerInstance, instanceCount, startIndex, baseVertexIndex, startInstance
        // renderArgsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        //
        // var args = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        //
        // args[0].indexCountPerInstance = (uint)renderMesh.GetIndexCount(0);
        // args[0].instanceCount = (uint)0;
        // args[0].startIndex = (uint)renderMesh.GetIndexStart(0);
        // args[0].baseVertexIndex = (uint)renderMesh.GetBaseVertex(0);
        // args[0].startInstance = 0;
        // renderArgsBuffer.SetData(args);
        
        renderArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        var args = new uint[5];
        args[0] = (uint)renderMesh.GetIndexCount(0);
        args[1] = (uint)0;
        args[2] = (uint)renderMesh.GetIndexStart(0);
        args[3] = (uint)renderMesh.GetBaseVertex(0);
        renderArgsBuffer.SetData(args);
        
    }
    
}
