using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Postprocess_Feature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Shader shader;
        private Material material;
        
        //构造函数
        public CustomRenderPass()
        {
            shader = Shader.Find("");
            if (shader == null)
            {
                Debug.LogWarningFormat("没有找到后处理Shader");
                return;
            }

            material = CoreUtils.CreateEngineMaterial(shader);
            if (material == null)
            {
                Debug.LogWarningFormat("Material创建失败");
                return;
            }
            
            
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.postProcessingEnabled == false)
            {
                Debug.LogWarningFormat("相机没开后处理还想要后处理雾？");
                return;
            }
            
            var sour = renderingData.cameraData.renderer.cameraColorTargetHandle;
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            int dest = shader.FindPropertyIndex("");
            
            CommandBuffer cmd = CommandBufferPool.Get("");
            
            cmd.GetTemporaryRT(dest, descriptor);
            
            cmd.Blit(sour, dest, material);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;
    
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

        renderer.EnqueuePass(m_ScriptablePass);
    }
}


