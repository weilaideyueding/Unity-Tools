using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Folder_Create_V1 : MonoBehaviour
{
    [MenuItem("Tools/My Tools/Folder_Create")]
    static void Folder_Create()
    {
        string prjPath = Application.dataPath + "/";
        Directory.CreateDirectory(prjPath + "Art");
        Directory.CreateDirectory(prjPath + "Art/Animation");
        Directory.CreateDirectory(prjPath + "Art/Audio");
        Directory.CreateDirectory(prjPath + "Art/Material");
        Directory.CreateDirectory(prjPath + "Art/Object");
        Directory.CreateDirectory(prjPath + "Art/Texture");
        
        Directory.CreateDirectory(prjPath + "Code");
        Directory.CreateDirectory(prjPath + "Code/Scripts");
        Directory.CreateDirectory(prjPath + "Code/Shaders");
        
        Directory.CreateDirectory(prjPath + "Prefabs");
        
        Directory.CreateDirectory(prjPath + "Resources");
        Directory.CreateDirectory(prjPath + "Resources/Characters");
        Directory.CreateDirectory(prjPath + "Resources/Managers");
        Directory.CreateDirectory(prjPath + "Resources/Props");
        Directory.CreateDirectory(prjPath + "Resources/UI");
        
        AssetDatabase.Refresh();
    }
}
