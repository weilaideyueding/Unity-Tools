using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

public class RampMapCreator : EditorWindow
{
    [MenuItem("Tools/My Tools/Art/创建Ramp图", priority = 1), MenuItem("Assets/Create/使用Asset创建/创建Ramp图", priority = 120)]
    private static void ShowWindow()
    {
        var window = GetWindow<RampMapCreator>();
        window.titleContent = new GUIContent("RampMapCreator");
        window.minSize = new Vector2(300, 400);
        window.Show();
    }
    
    [SerializeField]
    protected List<Gradient> _gradients = new List<Gradient>();
    protected SerializedObject _serializedObject;    // 序列化对象
    protected SerializedProperty _assetListProperty;   // 序列化属性
    
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);

        _assetListProperty = _serializedObject.FindProperty("_gradients");
        
        _preShader = Shader.Find("Editor/RampPreview");
        if (_preShader != null)
        {
            _preMaterial = CoreUtils.CreateEngineMaterial(_preShader);
        }


        if (Selection.activeObject != null)
        {
            Object obj = Selection.activeObject;
            

            if (obj.GetType() == typeof(RampMapData))
            {
                RampMapData asset = obj as RampMapData;
                
                _rampName = asset.rampMapName;
                _rampMapWidth = asset.rampMapWidth;
                _rampMapHeight = asset.rampMapHeight;
                _gradients = asset.gradients;
            }
            else
            {
                Debug.LogWarning("所选不是 RampMapData的 Asset文件");
                return;
            }
        }
    }

    private int _rampMapWidth = 32;
    private int _rampMapHeight = 4;
    private Texture2D _rampMap;
    private string _rampName;

    
    private string[] _mapFormat = { "TGA", "PNG", "JPG" };
    private int _mapindex = 0;
    private string _format = ".tga";


    private Texture2D _previewTex;
    private Material _preMaterial;
    private Shader _preShader;
    private float _preWidth = 1;
    private int _setp = 4;
    private float _rampY = 0;
    

    private TextureWrapMode _textureWrapMode = TextureWrapMode.Clamp;
    private FilterMode _filterMode = FilterMode.Point;
    
    Vector2 scrollPos;

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
            {
                DrawAssetGUI();
            }
            

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _rampMap = CreateRamp();
                SceneView.RepaintAll();

                if (_rampMap != null)
                {
                    PreviewTex();
                }


                Save(); 
            }
        }

    }

    
    

    void DrawAssetGUI()
    {
        
        _serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_assetListProperty);
        
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
        }

        _rampMapWidth = EditorGUILayout.IntField("每条渐变的宽度", _rampMapWidth);
        _rampMapHeight = EditorGUILayout.IntField("每条渐变的高度", _rampMapHeight);
    
        
        if (GUILayout.Button("Load Asset"))
        {
            string path = EditorUtility.OpenFilePanel("Load RampMapData", Application.dataPath, "asset");
            LoadConfig(path);
        }
        
        if (GUILayout.Button("Save Asset"))
        {
            string path = EditorUtility.SaveFilePanel("Save As", Application.dataPath, "RampMapData", "asset");
            SaveConfig(path);
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("预览", EditorStyles.boldLabel);

        var labelWidth = EditorGUIUtility.labelWidth;
        
        EditorGUIUtility.labelWidth = 50;
        _preWidth = EditorGUILayout.Slider("预览宽度", _preWidth, 0.1f, 15);
        _setp = EditorGUILayout.IntSlider("预览步长", _setp, 1, 10);
        _rampY = EditorGUILayout.Slider("采样Y轴", _rampY, 0, 1);
        EditorGUIUtility.labelWidth = labelWidth;


        if (GUI.changed)
        {

            _preMaterial = CoreUtils.CreateEngineMaterial(_preShader);

            Shader.SetGlobalTexture("_RampPreviewTex", _rampMap);
            _preMaterial.SetInt("sampleStep", _setp);
            _preMaterial.SetFloat("RampY", _rampY);
            _previewTex = AssetPreview.GetAssetPreview(_preMaterial);
        }
        
        GUILayout.Box(_previewTex, GUILayout.Width(200), GUILayout.Height(200));
        
    }
    
    Texture2D CreateRamp()
    {
        Texture2D ramp = new Texture2D(_rampMapWidth, _rampMapHeight * _gradients.Count, TextureFormat.RGBA32, false);
        ramp.filterMode = FilterMode.Bilinear;

        Color[] cols = new Color[_rampMapWidth];
        
        float inv = 1f / (_rampMapWidth - 1);

        for (int i = 0; i < _gradients.Count; i++)
        {
            int start = _rampMapHeight * i;
            int end = start + _rampMapHeight;

            for (int x = 0; x < _rampMapWidth; x++)
            {
                var t = x * inv;
                cols[x] = _gradients[i].Evaluate(t);
            }
            
            for (int y = start; y < end; y++)
            {
                ramp.SetPixels(0, y, _rampMapWidth, 1, cols);
            }
            
        }
        
        ramp.wrapMode = TextureWrapMode.Clamp;
        ramp.filterMode = FilterMode.Point;

        ramp.Apply();
        return ramp;
    }
    
    void PreviewTex()
    {
        var rect = EditorGUILayout.GetControlRect(true, _rampMapHeight * _gradients.Count * _preWidth);
        EditorGUI.DrawPreviewTexture(rect, _rampMap);
    }

    
    void Save()
    {

        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("RampMap名称", GUILayout.Width(100));
        _rampName = EditorGUILayout.TextField(_rampName);
        _mapindex = EditorGUILayout.Popup(_mapindex, _mapFormat);
        switch (_mapindex)
        {
            case 0:
                _format = ".tga";
                break;
            case 1:
                _format = ".png";
                break;
            case 2:
                _format = ".jpg";
                break;
        }
        
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(5);
        _textureWrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Warp模式",_textureWrapMode);
        GUILayout.Space(5);
        _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter模式", _filterMode);
        GUILayout.Space(5);

        
        GUILayout.Space(5);
        if (GUILayout.Button("Save"))
        {
            string path = EditorUtility.SaveFolderPanel("Save RampMap", "", "");

            _rampMap.wrapMode = TextureWrapMode.Clamp;

            byte[] pngData = _rampMap.EncodeToTGA();
            
            switch (_mapindex)
            {
                case 0:
                    pngData = _rampMap.EncodeToTGA();
                    break;
                case 1:
                    pngData = _rampMap.EncodeToPNG();
                    break;
                case 2:
                    pngData = _rampMap.EncodeToJPG();
                    break;
            }


            string filePath = path + "/" + _rampName + _format;
            File.WriteAllBytes(filePath, pngData);
            filePath = "Assets" + filePath.Substring(Application.dataPath.Length);
            
            AssetDatabase.Refresh();
            
            Debug.Log(filePath);
            TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.wrapMode = _textureWrapMode;
                textureImporter.filterMode = _filterMode;
                AssetDatabase.ImportAsset(filePath);
                Debug.Log("保存成功");
            }
            
            AssetDatabase.Refresh();
        }
    }
    
    private void LoadConfig(string path)
    {
        RampMapData asset = AssetDatabase.LoadAssetAtPath(
            "Assets" + path.Substring(Application.dataPath.Length), typeof(RampMapData)) as RampMapData;
        
        _rampName = asset.rampMapName;
        _rampMapWidth = asset.rampMapWidth;
        _rampMapHeight = asset.rampMapHeight;
        _gradients = asset.gradients;
    }
    
    private void SaveConfig(string path)
    {
        RampMapData asset = ScriptableObject.CreateInstance<RampMapData>();
        asset.rampMapName = _rampName;
        asset.rampMapWidth = _rampMapWidth;
        asset.rampMapHeight = _rampMapHeight;
        asset.gradients = _gradients;
        
        AssetDatabase.CreateAsset(asset, "Assets" + path.Substring(Application.dataPath.Length));
        AssetDatabase.Refresh();
    }
}
