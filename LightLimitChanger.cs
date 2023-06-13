using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightLimitChanger : EditorWindow
{
    bool DefaultUse = false;
    bool IsValueSave = false;
    float MaxLightValue = 1;
    float MinLightValue = 0;
    
    const string SHADER_KEY_LightMinLimit = "material._LightMinLimit";
    const string SHADER_KEY_LightMaxLimit = "material._LightMaxLimit";

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

        GUILayout.Label("\nパラメータ");
        DefaultUse = EditorGUILayout.Toggle("DefaultUse", DefaultUse);
        IsValueSave = EditorGUILayout.Toggle("SaveValue", IsValueSave);
        MaxLightValue = EditorGUILayout.FloatField("MaxLight", MaxLightValue);
        MinLightValue = EditorGUILayout.FloatField("MinLight", MinLightValue);

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
        var animClip = new AnimationClip();

        //アニメーションの生成
        createAnimationClip(avater, animClip, avater.name);
        AssetDatabase.CreateAsset(animClip, $"{savePath}.anim");



        AssetDatabase.SaveAssets();

        Debug.Log(savePath);
        Debug.Log(saveName);
    }
    private void createAnimationClip(GameObject parentObj, AnimationClip animClip, string avaterName)
    {
        Transform children = parentObj.GetComponent<Transform>();

        if (children.childCount == 0) return;

        foreach(Transform obj in children)
        {
            SkinnedMeshRenderer SkinnedMeshR = obj.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (SkinnedMeshR != null)
            {
                foreach(var mat in SkinnedMeshR.sharedMaterials)
                {
                    if(mat != null && (mat.shader.name.IndexOf("lilToon") != -1 || mat.shader.name.IndexOf("motchiri") != -1))
                    {
                        string path = getObjectPath(obj).Replace(avaterName + "/", "");
                        setAnimationKey(obj.gameObject, animClip, path, SHADER_KEY_LightMinLimit);
                        setAnimationKey(obj.gameObject, animClip, path, SHADER_KEY_LightMaxLimit);
                    }
                }
            }
            createAnimationClip(obj.gameObject, animClip, avaterName);
        }
    }
    private void setAnimationKey(GameObject obj, AnimationClip clip, string path, string shaderPropName)
    {
        clip.SetCurve(path, typeof(SkinnedMeshRenderer), shaderPropName, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
    }
    private string getObjectPath(Transform trans)
    {
        string path = trans.name;
        var parent = trans.parent;
        while (parent)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }
        return path;
    }
}
