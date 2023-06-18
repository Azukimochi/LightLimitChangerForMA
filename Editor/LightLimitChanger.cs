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

namespace io.github.azukimochi
{
    public sealed class LightLimitChanger : EditorWindow
    {
        public LightLimitChangeParameters Parameters = LightLimitChangeParameters.Default;
        public VRCAvatarDescriptor TargetAvatar;
        private bool _isOptionFoldoutOpen = false;


        private const string SHADER_KEY_LILTOON_LightMinLimit = "_LightMinLimit";
        private const string SHADER_KEY_LILTOON_LightMaxLimit = "_LightMaxLimit";
        private const string SHADER_KEY_LILTOON_MainHSVG = "_MainTexHSVG";

        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ParameterName_Saturation = "LightLimitSaturation";

        private const string GenerateObjectName = "Light Limit Changer";

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow()
        {
            var window = GetWindow<LightLimitChanger>("LightLimitChanger");
            window.minSize = window.maxSize = new Vector2(600, 270);
        }

        private void OnGUI()
        {
            GUILayout.Label("---- Select Avater / アバターを選択");
            EditorGUI.BeginChangeCheck();
            TargetAvatar = EditorGUILayout.ObjectField(" Avater", TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            if (EditorGUI.EndChangeCheck())
            {
                Parameters = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings) 
                    ? settings.Parameters 
                    : LightLimitChangeParameters.Default;
            }
            EditorGUILayout.Space();
            GUILayout.Label("---- Paramater / パラメータ");
            var param = Parameters;

            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            param.IsDefaultUse = EditorGUILayout.Toggle(" DefaultUse", param.IsDefaultUse);
            param.IsValueSave = EditorGUILayout.Toggle(" SaveValue", param.IsValueSave);
            param.MaxLightValue = EditorGUILayout.FloatField(" MaxLight", param.MaxLightValue);
            param.MinLightValue = EditorGUILayout.FloatField(" MinLight", param.MinLightValue);
            param.DefaultLightValue = EditorGUILayout.FloatField(" DefaultLight", param.DefaultLightValue);

            EditorGUIUtility.labelWidth = originalValue;
            using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, " Option / オプション" ))
            {
                if (group.IsOpen)
                {
                    param.AllowSaturationControl = EditorGUILayout.Toggle(" Saturation Control", param.AllowSaturationControl);
                }
            }

            Parameters = param;

            EditorGUILayout.Space();
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
            var avatar = TargetAvatar;

            var settings = avatar.GetComponentInChildren<LightLimitChangerSettings>(true);

            if (settings == null || !settings.IsValid())
            {
                var fileName = $"{TargetAvatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
                var savePath = EditorUtility.SaveFilePanelInProject("保存場所", System.IO.Path.GetFileNameWithoutExtension(fileName), System.IO.Path.GetExtension(fileName).Trim('.'), "アセットの保存場所");
                if (string.IsNullOrEmpty(savePath))
                {
                    throw new Exception("キャンセルされました");
                }

                var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
                AssetDatabase.CreateAsset(fx, savePath);

                var obj = avatar.gameObject.GetOrAddChild(GenerateObjectName);
                var parameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();

                ConfigureLightControl(fx, parameters);
                ConfigureSaturationControl(fx, parameters);

                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();
                menuInstaller.menuToAppend = CreateMenu(fx);
                var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.deleteAttachedAnimator = true;
                mergeAnimator.animator = fx;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;

                settings = obj.GetOrAddComponent<LightLimitChangerSettings>();
                settings.FX = fx;
            }
            else
            {
                var fx = settings.FX as AnimatorController;
                fx.parameters = Array.Empty<AnimatorControllerParameter>();
                var parameters = settings.GetComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();
                var menuInstaller = settings.gameObject.GetOrAddComponent<ModularAvatarMenuInstaller>();
                menuInstaller.menuToAppend = CreateMenu(fx);

                fx.ClearSubAssets();
                fx.ClearLayers();

                ConfigureLightControl(fx, parameters);
                ConfigureSaturationControl(fx, parameters);
            }
            settings.Parameters = Parameters;
            AssetDatabase.SaveAssets();
        }

        private void ConfigureLightControl(AnimatorController fx, ModularAvatarParameters parameters)
        {
            AnimationClip defaultAnim = new AnimationClip() { name = "Default Light" }.AddTo(fx);
            AnimationClip anim = new AnimationClip() { name = "Change Light" }.AddTo(fx);

            var avatar = TargetAvatar;
            var linearCurve = AnimationCurve.Linear(0, Parameters.MinLightValue, 1 / 60f, Parameters.MaxLightValue);

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    var type = renderer.GetType();
                    bool isLilToon = false;

                    (float Min, float Max) defaultValue = (0, 1);

                    foreach (var material in renderer.sharedMaterials)
                    {
                        var shader = material?.shader;
                        if (shader != null)
                        {
                            int count = shader.GetPropertyCount();
                            for (int i = 0; i < count; i++)
                            {
                                var propertyName = shader.GetPropertyName(i);
                                if (propertyName == SHADER_KEY_LILTOON_LightMinLimit)
                                {
                                    isLilToon = true;
                                    defaultValue.Min = material.GetFloat(propertyName);
                                }
                                if (propertyName == SHADER_KEY_LILTOON_LightMaxLimit)
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
                        var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMinLimit}", AnimationCurve.Constant(0, 0, defaultValue.Min));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMaxLimit}", AnimationCurve.Constant(0, 0, defaultValue.Max));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMinLimit}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMaxLimit}", linearCurve);
                    }
                }
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

            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = Parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = Parameters.DefaultLightValue, type = AnimatorControllerParameterType.Float });
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = Parameters.IsValueSave, defaultValue = Parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Value, saved = Parameters.IsValueSave, defaultValue = Parameters.DefaultLightValue, syncType = ParameterSyncType.Float });

        }

        private void ConfigureSaturationControl(AnimatorController fx, ModularAvatarParameters parameters)
        {
            if (!Parameters.AllowSaturationControl)
                return;

            AnimationClip defaultAnim = new AnimationClip() { name = "Default Saturation" }.AddTo(fx);
            AnimationClip anim = new AnimationClip() { name = "Change Saturation" }.AddTo(fx);

            var avatar = TargetAvatar;
            var linearCurve = AnimationCurve.Linear(0, 0, 1 / 60f, 2);

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    var type = renderer.GetType();
                    bool isLilToon = false;

                    Color defaultValue = new Color(0, 1, 1, 1);

                    foreach (var material in renderer.sharedMaterials)
                    {
                        var shader = material?.shader;
                        if (shader != null)
                        {
                            int count = shader.GetPropertyCount();
                            for (int i = 0; i < count; i++)
                            {
                                var propertyName = shader.GetPropertyName(i);
                                if (propertyName == SHADER_KEY_LILTOON_MainHSVG)
                                {
                                    isLilToon = true;
                                    defaultValue = material.GetColor(propertyName);
                                }
                            }
                        }
                        if (isLilToon)
                            break;
                    }
                    if (isLilToon)
                    {
                        var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.r", AnimationCurve.Constant(0, 0, defaultValue.r));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.g", AnimationCurve.Constant(0, 0, defaultValue.g));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.b", AnimationCurve.Constant(0, 0, defaultValue.b));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.a", AnimationCurve.Constant(0, 0, defaultValue.a));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.r", AnimationCurve.Constant(0, 0, defaultValue.r));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.g", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.b", AnimationCurve.Constant(0, 0, defaultValue.b));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.a", AnimationCurve.Constant(0, 0, defaultValue.a));
                    }
                }
            }

            var layer = new AnimatorControllerLayer() { name = "Saturation", defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = defaultAnim }.HideInHierarchy().AddTo(fx);
            var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = anim, timeParameterActive = true, timeParameter = ParameterName_Saturation }.HideInHierarchy().AddTo(fx);

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

            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Saturation, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Saturation, saved = Parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
        }

        private VRCExpressionsMenu CreateMenu(AnimatorController fx)
        {
            var mainMenu = new VRCExpressionsMenu
            {
                name = "Main Menu",
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
            }.AddTo(fx);

            if (Parameters.AllowSaturationControl)
            {
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Saturation",
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    subParameters = new VRCExpressionsMenu.Control.Parameter[]
                    {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Saturation
                        }
                    },
                });
            }

            var rootMenu = new VRCExpressionsMenu()
            {
                name = "Root Menu",
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "Light Limit Changer",
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = mainMenu,
                    },
                },

            }.AddTo(fx);



            return rootMenu;
        }
    }
}

#endif
