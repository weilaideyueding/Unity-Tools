using UnityEditor;
using UnityEngine;

public class ShaderGUIName : ShaderGUI
{
    private Material target;
    private MaterialEditor editor;
    private MaterialProperty[] property;
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        editor = materialEditor;
        property = properties;
        target = materialEditor.target as Material;
        
        base.OnGUI(materialEditor, properties);
    }
}

