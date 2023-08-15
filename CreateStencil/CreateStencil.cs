using UnityEditor;
public class CreateStencil : Editor 
{
    private const string baseShaderPath = "Assets/Editor/CreateStencil/URP_Base.shader";
    [MenuItem("Assets/Create/Shader/URP/BaseShader")]
    static void CreateBaseShader()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(baseShaderPath,"URP_Base.shader");
    }

    private const string postprocessFeaturePath = "Assets/Editor/CreateStencil/Postprocess_Feature.cs";
    [MenuItem("Assets/Create/My Stencil/PostprocessFeature", priority = 11)]
    static void CreatePostprocessFeature()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(postprocessFeaturePath,"PostprocessFeature.cs");
    }
    
    private const string baseVolumeComponent = "Assets/Editor/CreateStencil/BaseVolumeComponent.cs";
    [MenuItem("Assets/Create/My Stencil/BaseVolumeComponent", priority = 12)]
    static void CreateBaseVolumeComponent()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(baseVolumeComponent,"BaseVolumeComponent.cs");
    }
    
    
}


