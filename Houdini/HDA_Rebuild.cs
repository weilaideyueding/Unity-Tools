using HoudiniEngineUnity;
using UnityEditor;
using UnityEngine;

// 支持单个 HDA重构，和多个 HDA重构

public class HDA_Rebuild : EditorWindow
{
    [MenuItem("Tools/Houdini/重构HDA")]
    private static void ReBuildHDA()
    {
        HDA_Rebuild window = GetWindow<HDA_Rebuild>("重构HDA");
        window.minSize = new Vector2(300, 50);
        window.SelectGameObject();
        window.Show();
    }
    
    private GameObject[] selectObj;
    private GameObject obj;
    
    // 获取选择
    public void SelectGameObject()
    {
        // 如果当前有选择
        if (Selection.gameObjects != null)
        {
            selectObj = Selection.gameObjects;
        }
    }


    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("如果需要多个的话，先在 Hierarchy中选择好HDA，再使用工具");
        GUILayout.Space(10);

        // 如果有没有选择物体的话，绘制选择框
        if (selectObj.Length == 0)
        {
            obj = (GameObject)EditorGUILayout.ObjectField("单个重构的HDA", obj, typeof(GameObject), true);
        }
        else
        {
            // 提醒用户
            EditorGUILayout.LabelField("您当前已在 Hierarchy中选好了物体，直接重构就好");
        }

        GUILayout.Space(10);
        
        // 绘制按钮
        if (GUILayout.Button("确认重构"))
        {
            // 如果没有选择物体
            if (selectObj.Length == 0)
            {
                Rebuild(obj);
            }else
            {
                for (int i = 0; i < selectObj.Length; i++)
                {
                    Rebuild(selectObj[i]);
                }
            }

        }
    }

    // 执行Rebuilt
    private void Rebuild(GameObject o)
    {
        HEU_HoudiniAsset hda = o.GetComponent<HEU_HoudiniAssetRoot>().HoudiniAsset;
        hda.RequestReload();
    }
}
