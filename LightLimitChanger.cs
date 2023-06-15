#if UNITY_EDITOR

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;
using System.Linq;

namespace io.github.azukimochi
{
    public sealed class LightLimitChanger : EditorWindow
    {
        public bool IsDefaultUse = false;
        public bool IsValueSave = false;
        public float DefaultLightValue = 0.5f;
        public float MaxLightValue = 1.0f;
        public float MinLightValue = 0.0f;
        public VRCAvatarDescriptor TargetAvatar;

        private const string SHADER_KEY_LightMinLimit = "_LightMinLimit";
        private const string SHADER_KEY_LightMaxLimit = "_LightMaxLimit";
        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ArtifactsFolderGUID = "f60eb27a31a2bb640a2b06ae64950203";

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        static void CreateGUI()
        {
            GetWindow<LightLimitChanger>("LightLimitChanger");
        }
        private void OnGUI()
        {
            GUILayout.Label("---- Select Avater / アバターを選択");
            TargetAvatar = EditorGUILayout.ObjectField(" Avater", TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            EditorGUILayout.Space();
            GUILayout.Label("---- Paramater / パラメータ");
            IsDefaultUse = EditorGUILayout.Toggle(" DefaultUse", IsDefaultUse);
            IsValueSave = EditorGUILayout.Toggle(" SaveValue", IsValueSave);
            MaxLightValue = EditorGUILayout.FloatField(" MaxLight", MaxLightValue);
            MinLightValue = EditorGUILayout.FloatField(" MinLight", MinLightValue);
            DefaultLightValue = EditorGUILayout.FloatField(" DefaultLight", DefaultLightValue);

            EditorGUI.BeginDisabledGroup(TargetAvatar == null);
            if (GUILayout.Button(" Generate / 生成 "))
            {
                infoLabel = "生成中・・・";
                try
                {
                    GenerateAssets();
                    infoLabel = "生成終了";
                }
                catch (Exception e)
                {
                    infoLabel = $"エラー: {e.Message}";
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Label(infoLabel);

        }

        private void GenerateAssets()
        {
            var folder = AssetDatabase.GUIDToAssetPath(ArtifactsFolderGUID);
            if (folder == null)
            {
                folder = "Assets/LightLimitChangerForMA/Artifacts/";
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder("Assets/LightLimitChangerForMA", "Artifacts");
                }
            }
            if (!AssetDatabase.IsValidFolder(folder))
            {
                throw new System.IO.DirectoryNotFoundException($"Artifacts folder is not found");
            }

            var avatar = TargetAvatar;

            var fileName = $"{TargetAvatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
            var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
            AssetDatabase.CreateAsset(fx, System.IO.Path.Combine(folder, fileName));
            AnimationClip defaultAnim = null;
            AnimationClip anim = null;

            var linearCurve = AnimationCurve.Linear(0, MinLightValue, 1 / 60f, MaxLightValue);

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    var type = renderer.GetType();
                    bool isLilToon = false;

                    (float Min, float Max) defaultValue = (0, 1);

                    foreach(var material in renderer.sharedMaterials)
                    {
                        var shader = material?.shader;
                        if (shader != null)
                        {
                            int count = shader.GetPropertyCount();
                            for(int i = 0; i < count; i++)
                            {
                                var propertyName = shader.GetPropertyName(i);
                                if (propertyName == SHADER_KEY_LightMinLimit)
                                {
                                    isLilToon = true;
                                    defaultValue.Min = material.GetFloat(propertyName);
                                }
                                if (propertyName == SHADER_KEY_LightMaxLimit)
                                {
                                    isLilToon = true;
                                    defaultValue.Max = material.GetFloat(propertyName);
                                }
                            }
                        }
                        if (isLilToon)
                            break;
                    }
                    if (isLilToon)
                    {
                        if (defaultAnim == null || anim == null)
                        {
                            defaultAnim = new AnimationClip().AddTo(fx);
                            anim = new AnimationClip().AddTo(fx);
                        }

                        var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMinLimit}", AnimationCurve.Constant(0, 0, defaultValue.Min));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMaxLimit}", AnimationCurve.Constant(0, 0, defaultValue.Max));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMinLimit}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LightMaxLimit}", linearCurve);
                    }
                }
            }

            if (defaultAnim == null || anim == null)
            {
                return;
            }

            var layer = new AnimatorControllerLayer() { name = "Light", defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = defaultAnim }.HideInHierarchy().AddTo(fx);
            var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = anim, timeParameterActive = true, timeParameter = ParameterName_Value }.HideInHierarchy().AddTo(fx);

            var condition = new AnimatorCondition[] { new AnimatorCondition() { parameter = ParameterName_Toggle, mode = AnimatorConditionMode.If, threshold = 0 } };

            var tr = new AnimatorStateTransition()
            {
                destinationState = state,
                duration = 0,
                hasExitTime = false,
                conditions = condition,
            }.HideInHierarchy().AddTo(fx);

            defaultState.AddTransition(tr);

            condition[0].mode = AnimatorConditionMode.IfNot;
            tr = new AnimatorStateTransition()
            {
                destinationState = defaultState,
                duration = 0,
                hasExitTime = false,
                conditions = condition,
            }.HideInHierarchy().AddTo(fx);

            state.AddTransition(tr);

            stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(0, 200));
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(0, 350));

            fx.AddLayer(layer);
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = IsDefaultUse, type = AnimatorControllerParameterType.Bool });
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = DefaultLightValue, type = AnimatorControllerParameterType.Float });

            var menu = new VRCExpressionsMenu()
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control 
                    {
                        name = "Light Limit Changer",
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = new VRCExpressionsMenu
                        {
                            controls = new List<VRCExpressionsMenu.Control>
                            {
                                new VRCExpressionsMenu.Control
                                {
                                    name = "Enable",
                                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                                    parameter = new VRCExpressionsMenu.Control.Parameter
                                    {
                                        name = ParameterName_Toggle,
                                    },
                                },
                                new VRCExpressionsMenu.Control
                                {
                                    name = "Light",
                                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                                    subParameters = new VRCExpressionsMenu.Control.Parameter[]
                                    {
                                        new VRCExpressionsMenu.Control.Parameter
                                        {
                                            name = ParameterName_Value
                                        }
                                    },
                                },
                            },
                        }.AddTo(fx),
                    },
                },
            }.AddTo(fx);

            var prefab = new GameObject("Light Limit Changer");
            prefab.transform.parent = avatar.transform;

            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = menu;
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.animator = fx;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = IsValueSave, defaultValue = IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Value, saved = IsValueSave, defaultValue = DefaultLightValue, syncType = ParameterSyncType.Float });
        }
    }

    internal static class Utils
    {
        public static T AddTo<T>(this T obj, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(obj, asset);
            return obj;
        }

        public static T HideInHierarchy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            return obj;
        }

        public static string GetRelativePath(this Transform transform, Transform root, bool includeRelativeTo = false)
        {
            var buffer = _relativePathBuffer;
            if (buffer is null)
            {
                buffer = _relativePathBuffer = new string[128];
            }

            var t = transform;
            int idx = buffer.Length;
            while (t != null && t != root)
            {
                buffer[--idx] = t.name;
                t = t.parent;
            }
            if (includeRelativeTo && t != null && t == root)
            {
                buffer[--idx] = t.name;
            }

            return string.Join("/", buffer, idx, buffer.Length - idx);
        }

        private static string[] _relativePathBuffer;
    }
}

#endif
