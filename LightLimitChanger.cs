using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightLimitChanger : EditorWindow
{
    const string shaderkeyword_AsUnlit = "material._AsUnlit";
    const string shaderkeyword_LightMinLimit = "material._LightMinLimit";
    const string shaderkeyword_LightMaxLimit = "material._LightMaxLimit";

    GameObject avater;

    [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
    static void CreateGUI()
    {
        GetWindow<LightLimitChanger>("LightLimitChanger");
    }
    private void OnGUI()
    {
        GUILayout.Label("アバターを選択");
        avater = (GameObject)EditorGUILayout.ObjectField("Avater", avater, typeof(GameObject), true);

        EditorGUI.BeginDisabledGroup(avater == null);
        if(GUILayout.Button("Generate"))
        {
            var path = EditorUtility.SaveFilePanelInProject("保存場所", "New Assets", "asset", "アセットの保存場所");
            if (path == null) return;

            path = new System.Text.RegularExpressions.Regex(@"\.asset").Replace(path, "");
            var saveName = System.IO.Path.GetFileNameWithoutExtension(path);

            GenerateAssets(path, saveName, avater);
        }
        EditorGUI.EndDisabledGroup();
    }
    private void GenerateAssets(string savePath, string saveName, GameObject avater) 
    {
        Debug.Log(savePath);
        Debug.Log(saveName);
    }
}
