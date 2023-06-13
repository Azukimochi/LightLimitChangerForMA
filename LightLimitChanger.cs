using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;

#if UNITY_EDITOR
public class LightLimitChanger : EditorWindow
{
    bool isDefaultUse = false;
    bool isValueSave = false;
    float defaultLightValue = 0.5f;
    float MaxLightValue = 1.0f;
    float MinLightValue = 0.0f;

    const string SHADER_KEY_LightMinLimit = "material._LightMinLimit";
    const string SHADER_KEY_LightMaxLimit = "material._LightMaxLimit";

    GameObject avater;

    string infoLabel = "";

    [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
    static void CreateGUI()
    {
        GetWindow<LightLimitChanger>("LightLimitChanger");
    }
    private void OnGUI()
    {
        GUILayout.Label("---- Select Avater / アバターを選択");
        avater = (GameObject)EditorGUILayout.ObjectField(" Avater", avater, typeof(GameObject), true);
        EditorGUILayout.Space();
        GUILayout.Label("---- Paramater / パラメータ");
        isDefaultUse = EditorGUILayout.Toggle(" DefaultUse", isDefaultUse);
        isValueSave = EditorGUILayout.Toggle(" SaveValue", isValueSave);
        MaxLightValue = EditorGUILayout.FloatField(" MaxLight", MaxLightValue);
        MinLightValue = EditorGUILayout.FloatField(" MinLight", MinLightValue);
        defaultLightValue = EditorGUILayout.FloatField(" DefaultLight", defaultLightValue);

        EditorGUI.BeginDisabledGroup(avater == null);
        if (GUILayout.Button(" Generate / 生成 "))
        {
            var path = EditorUtility.SaveFilePanelInProject("保存場所", $"{avater.name} Light Change", "asset", "アセットの保存場所");
            if (path == null) return;

            path = new System.Text.RegularExpressions.Regex(@"\.asset").Replace(path, "");
            var saveName = System.IO.Path.GetFileNameWithoutExtension(path);

            infoLabel = "生成中・・・";
            GenerateAssets(path, saveName, avater);
            infoLabel = "生成終了";
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.Label(infoLabel);
       
    }
    private void GenerateAssets(string savePath, string saveName, GameObject avater)
    {
        /////////////////////
        //アニメーションの生成
        var animLightChangeClip = new AnimationClip();
        var animLightDisableClip = new AnimationClip();

        //アニメーションをまとめて生成
        createAnimationClip(avater, animLightChangeClip, animLightDisableClip, avater.name);
        AssetDatabase.CreateAsset(animLightChangeClip, $"{savePath}_active.anim");
        AssetDatabase.CreateAsset(animLightDisableClip, $"{savePath}_deactive.anim");

        //////////////////////////////////
        //アニメーターコントローラーの生成
        var controller = AnimatorController.CreateAnimatorControllerAtPath($"{savePath}.controller");

        controller.parameters = new AnimatorControllerParameter[]
        {
            new AnimatorControllerParameter()
            {
                name = saveName,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = this.isDefaultUse
            },
            new AnimatorControllerParameter()
            {
                name = saveName + "_motion",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = this.defaultLightValue
            }
        };
        //レイヤーとステートの設定
        var layer = controller.layers[0];
        layer.name = saveName;
        layer.stateMachine.name = saveName;

        var lightActiveState = layer.stateMachine.AddState($"{saveName}_Active", new Vector3(300, 200));
        lightActiveState.motion = animLightChangeClip;
        lightActiveState.writeDefaultValues = false;
        lightActiveState.timeParameterActive = true;
        lightActiveState.timeParameter = saveName + "_motion";

        var lightDeActiveState = layer.stateMachine.AddState($"{saveName}_DeActive", new Vector3(300, 100));
        lightDeActiveState.motion = animLightDisableClip;
        lightDeActiveState.writeDefaultValues = false;
        layer.stateMachine.defaultState = lightDeActiveState;

        var toDeActiveTransition = lightActiveState.AddTransition(lightDeActiveState);
        toDeActiveTransition.exitTime = 0;
        toDeActiveTransition.duration = 0;
        toDeActiveTransition.hasExitTime = false;
        toDeActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = saveName,
                    threshold = 1,
                },
            };
        var toActiveTransition = lightDeActiveState.AddTransition(lightActiveState);
        toActiveTransition.exitTime = 0;
        toActiveTransition.duration = 0;
        toActiveTransition.hasExitTime = false;
        toActiveTransition.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.If,
                    parameter = saveName,
                    threshold = 1,
                },
            };

        //////////////////////
        //ExpressionMenuの生成

        //SubMenuの生成
        var subMenu = new VRCExpressionsMenu
        {
            controls = new List<VRCExpressionsMenu.Control>
                {
                new VRCExpressionsMenu.Control {
                        name = saveName + " Toggle",
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = saveName,
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                new VRCExpressionsMenu.Control {
                    name = saveName + " light",
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    subParameters = new VRCExpressionsMenu.Control.Parameter[] {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = saveName + "_motion"
                        }
                    },
                    value = defaultLightValue,
                    labels = new VRCExpressionsMenu.Control.Label[] { },
                },
             },
        };
        AssetDatabase.CreateAsset(subMenu, $"{savePath}_subMenu.asset");

        //Menuの生成
        var menu = new VRCExpressionsMenu()
        {
            controls = new List<VRCExpressionsMenu.Control>
                {
                new VRCExpressionsMenu.Control {
                        name = saveName,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = subMenu,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
             },
        };
        AssetDatabase.CreateAsset(menu, $"{savePath}.asset");

        //////////////
        //Prefabの生成

        var prefab = new GameObject(saveName);
        var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
        menuInstaller.menuToAppend = menu;
        var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();

        //MA Parameters
        parameters.parameters.Add(new ParameterConfig
        {
            nameOrPrefix = saveName,
            defaultValue = (float)Convert.ToDouble(this.isDefaultUse),
            syncType = ParameterSyncType.Bool,
            saved = this.isValueSave
        });
        parameters.parameters.Add(new ParameterConfig
        {
            nameOrPrefix = name = saveName + "_motion",
            defaultValue = this.defaultLightValue,
            syncType = ParameterSyncType.Float,
            saved = this.isValueSave
        });
        // MA MergeAnimator
        var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
        mergeAnimator.animator = controller;
        mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
        mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
        mergeAnimator.matchAvatarWriteDefaults = true;
        PrefabUtility.SaveAsPrefabAsset(prefab, $"{savePath}.prefab");
        DestroyImmediate(prefab);

        AssetDatabase.SaveAssets();

        Debug.Log(savePath);
        Debug.Log(saveName);
    }
    private void createAnimationClip(GameObject parentObj, AnimationClip animActiveClip, AnimationClip animDeActiveClip, string avaterName)
    {
        Transform children = parentObj.GetComponent<Transform>();

        if (children.childCount == 0) return;

        foreach (Transform obj in children)
        {
            SkinnedMeshRenderer SkinnedMeshR = obj.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (SkinnedMeshR != null)
            {
                foreach (var mat in SkinnedMeshR.sharedMaterials)
                {
                    if (mat != null && (mat.shader.name.IndexOf("lilToon") != -1 || mat.shader.name.IndexOf("motchiri") != -1))
                    {
                        string path = getObjectPath(obj).Replace(avaterName + "/", "");
                        animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));
                        animActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, MinLightValue), new Keyframe(1 / 60.0f, MaxLightValue)));

                        animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMinLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 0.0f), new Keyframe(1 / 60.0f, 0.0f)));
                        animDeActiveClip.SetCurve(path, typeof(SkinnedMeshRenderer), SHADER_KEY_LightMaxLimit, new AnimationCurve(new Keyframe(0 / 60.0f, 1.0f), new Keyframe(1 / 60.0f, 1.0f)));
                    }
                }
            }
            createAnimationClip(obj.gameObject, animActiveClip, animDeActiveClip, avaterName);
        }
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
#endif
