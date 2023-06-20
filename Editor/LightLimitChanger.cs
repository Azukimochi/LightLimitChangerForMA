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
        private bool _isVersionInfoFoldoutOpen = false;

        private const string SHADER_KEY_LILTOON_LightMinLimit = "_LightMinLimit";
        private const string SHADER_KEY_LILTOON_LightMaxLimit = "_LightMaxLimit";
        private const string SHADER_KEY_LILTOON_MainHSVG = "_MainTexHSVG";

        private const string SHADER_KEY_SUNAO_MinimumLight = "_MinimumLight";
        private const string SHADER_KEY_SUNAO_DirectionalLight = "_DirectionalLight";
        private const string SHADER_KEY_SUNAO_PointLight = "_PointLight";
        private const string SHADER_KEY_SUNAO_SHLight = "_SHLight";

        private const string SHADER_KEY_POIYOMI_LightingMinLightBrightness = "_LightingMinLightBrightness";
        private const string SHADER_KEY_POIYOMI_LightingCap = "_LightingCap";
        private const string SHADER_KEY_POIYOMI_MainColorAdjustToggle = "_MainColorAdjustToggle";
        private const string SHADER_KEY_POIYOMI_Saturation = "_Saturation";

        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ParameterName_Saturation = "LightLimitSaturation";
        private const string ParameterName_Reset = "LightLimitReset";

        private const string GenerateObjectName = "Light Limit Changer";

        public const string Title = "Light Limit Changer For MA";
        public static string Version = string.Empty;

        private static readonly string[] _targetShaderLabels = Enum.GetNames(typeof(TargetShaders));

        private string infoLabel = "";

        [MenuItem("Tools/Modular Avatar/LightLimitChanger")]
        public static void CreateWindow()
        {
            var window = GetWindow<LightLimitChanger>(Title);
            window.minSize = new Vector2(380, 400);
            window.maxSize = new Vector2(1000, 400);
        }

        private void OnEnable()
        {
            Version = Utils.GetVersion();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            ShowVersionInfo();
            EditorGUILayout.Separator();
            ShowGeneratorMenu();
            EditorGUILayout.Separator();
            ShowSettingsMenu();
        }

        private void ShowGeneratorMenu()
        {
            using (new Utils.DisabledScope(EditorApplication.isPlaying))
            {
                using (new Utils.GroupScope(Localization.S("Select Avatar"), 180))
                {
                    EditorGUI.BeginChangeCheck();
                    TargetAvatar = EditorGUILayout.ObjectField(Localization.S("Avatar"), TargetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Parameters = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings)
                            ? settings.Parameters
                            : LightLimitChangeParameters.Default;
                    }
                }

                using (new Utils.GroupScope(Localization.S("Parameter"), 180))
                {
                    var param = Parameters;

                    param.IsDefaultUse = EditorGUILayout.Toggle(Localization.S( "DefaultUse"), param.IsDefaultUse);
                    param.IsValueSave = EditorGUILayout.Toggle(Localization.S("SaveValue"), param.IsValueSave);
                    param.MaxLightValue = EditorGUILayout.FloatField(Localization.S("MaxLight"), param.MaxLightValue);
                    param.MinLightValue = EditorGUILayout.FloatField(Localization.S("MinLight"), param.MinLightValue);
                    param.DefaultLightValue = EditorGUILayout.FloatField(Localization.S("DefaultLight"), param.DefaultLightValue);

                    using (var group = new Utils.FoldoutHeaderGroupScope(ref _isOptionFoldoutOpen, Localization.S("Options")))
                    {
                        if (group.IsOpen)
                        {
                            param.TargetShader = (TargetShaders)EditorGUILayout.MaskField(Localization.S("Target Shader"), (int)param.TargetShader, _targetShaderLabels);
                            param.AllowSaturationControl = EditorGUILayout.Toggle(Localization.S("Allow Saturation Control"), param.AllowSaturationControl);
                            param.AddResetButton = EditorGUILayout.Toggle(Localization.S("Add Resset Button"), param.AddResetButton);
                        }
                    }

                    Parameters = param;
                }
                
                using (new Utils.DisabledScope(TargetAvatar == null || Parameters.TargetShader == 0))
                {
                    string buttonLabel;
                    {
                        buttonLabel = TargetAvatar != null && TargetAvatar.TryGetComponentInChildren<LightLimitChangerSettings>(out var settings) && settings.IsValid()
                        ? "Regenerate"
                        : "Generate";
                    }

                    if (GUILayout.Button(Localization.S(buttonLabel)))
                    {
                        infoLabel = Localization.S( "Processing");
                        try
                        {
                            GenerateAssets();
                            infoLabel = Localization.S("Complete");
                        }
                        catch (Exception e)
                        {
                            infoLabel = $"{Localization.S("Error")}: {e.Message}";
                        }
                    }
                }

                GUILayout.Label(infoLabel);
            }
        }

        private void ShowSettingsMenu()
        {
            Localization.ShowLocalizationUI();
        }

        private void ShowVersionInfo()
        {
            using (var foldout = new Utils.FoldoutHeaderGroupScope(ref _isVersionInfoFoldoutOpen, $"{Title} {Version}"))
            {
                if (foldout.IsOpen)
                {
                    DrawWebButton("BOOTH", "https://mochis-factory.booth.pm/items/4864776");
                    DrawWebButton("GitHub", "https://github.com/Azukimochi/LightLimitChangerForMA");
                }
            }
            
        }

        private void GenerateAssets()
        {
            var avatar = TargetAvatar;

            var settings = avatar.GetComponentInChildren<LightLimitChangerSettings>(true);

            if (settings == null || !settings.IsValid())
            {
                var fileName = $"{TargetAvatar.name}_{DateTime.Now:yyyyMMddHHmmss}_{GUID.Generate()}.controller";
                var savePath = EditorUtility.SaveFilePanelInProject(Localization.S("Save"), System.IO.Path.GetFileNameWithoutExtension(fileName), System.IO.Path.GetExtension(fileName).Trim('.'), Localization.S("Save Location"));
                if (string.IsNullOrEmpty(savePath))
                {
                    throw new Exception(Localization.S("Cancelled"));
                }

                var fx = new AnimatorController() { name = System.IO.Path.GetFileName(fileName) };
                AssetDatabase.CreateAsset(fx, savePath);

                var obj = avatar.gameObject.GetOrAddChild(GenerateObjectName);
                var parameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();

                ConfigureLightControl(fx, parameters);
                ConfigureSaturationControl(fx, parameters);
                ConfigureResetParamerters(fx, parameters);

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
                ConfigureResetParamerters(fx, parameters);
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
                    bool hasLilToon = false;
                    bool hasSunaoShader = false;
                    bool hasPoiyomiShader = false;

                    (float Min, float Max) lilDefaultValue = (0, 1);
                    (float Minimum, float Directional, float Point, float SH) sunaoDefaultValue = (0, 1, 1, 1);
                    (float Min, float Max) poiyomiDefaultValue = (0, 1);

                    foreach (var material in renderer.sharedMaterials)
                    {
                        var shader = material?.shader;
                        if (shader != null)
                        {
                            int count = shader.GetPropertyCount();
                            for (int i = 0; i < count; i++)
                            {
                                var propertyName = shader.GetPropertyName(i);
                                switch (propertyName)
                                {
                                    case SHADER_KEY_LILTOON_LightMinLimit:
                                        lilDefaultValue.Min = material.GetFloat(propertyName);
                                        goto lilToon;
                                    case SHADER_KEY_LILTOON_LightMaxLimit:
                                        lilDefaultValue.Max = material.GetFloat(propertyName);
                                        goto lilToon;

                                    lilToon:
                                        hasLilToon = true;
                                        break;

                                    case SHADER_KEY_SUNAO_MinimumLight:
                                        sunaoDefaultValue.Minimum = material.GetFloat(propertyName);
                                        goto sunao;

                                    case SHADER_KEY_SUNAO_DirectionalLight:
                                        sunaoDefaultValue.Directional = material.GetFloat(propertyName);
                                        goto sunao;

                                    case SHADER_KEY_SUNAO_PointLight:
                                        sunaoDefaultValue.Point = material.GetFloat(propertyName);
                                        goto sunao;

                                    case SHADER_KEY_SUNAO_SHLight:
                                        sunaoDefaultValue.SH = material.GetFloat(propertyName);
                                        goto sunao;

                                    sunao:
                                        hasSunaoShader = true;
                                        break;

                                    case SHADER_KEY_POIYOMI_LightingMinLightBrightness:
                                        poiyomiDefaultValue.Min = material.GetFloat(propertyName);
                                        goto poiyomi;

                                    case SHADER_KEY_POIYOMI_LightingCap:
                                        poiyomiDefaultValue.Max = material.GetFloat(propertyName);
                                        goto poiyomi;

                                    poiyomi:
                                        hasPoiyomiShader = true;
                                        break;
                                }
                            }
                        }
                    }
                    var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                    var targetShader = Parameters.TargetShader;
                    if (hasLilToon && targetShader.HasFlag(TargetShaders.lilToon))
                    {
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMinLimit}", AnimationCurve.Constant(0, 0, lilDefaultValue.Min));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMaxLimit}", AnimationCurve.Constant(0, 0, lilDefaultValue.Max));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMinLimit}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_LightMaxLimit}", linearCurve);
                    }
                    if (hasSunaoShader && targetShader.HasFlag(TargetShaders.Sunao))
                    {
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_MinimumLight}", AnimationCurve.Constant(0, 0, sunaoDefaultValue.Minimum));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_DirectionalLight}", AnimationCurve.Constant(0, 0, sunaoDefaultValue.Directional));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_PointLight}", AnimationCurve.Constant(0, 0, sunaoDefaultValue.Point));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_SHLight}", AnimationCurve.Constant(0, 0, sunaoDefaultValue.SH));

                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_MinimumLight}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_DirectionalLight}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_PointLight}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_SUNAO_SHLight}", linearCurve);
                    }
                    if (hasPoiyomiShader && targetShader.HasFlag(TargetShaders.Poiyomi))
                    {
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", AnimationCurve.Constant(0, 0, poiyomiDefaultValue.Min));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_LightingCap}", AnimationCurve.Constant(0, 0, poiyomiDefaultValue.Max));

                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", linearCurve);
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_LightingCap}", linearCurve);
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

            stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(-20, 100));

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

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    var type = renderer.GetType();
                    bool hasLilToon = false;
                    bool hasPoiyomiShader = false;

                    Color lilDefaultValue = new Color(0, 1, 1, 1);
                    (float Saturation, int Enable) poiyomiDefaultValue = (0, 0);

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
                                    hasLilToon = true;
                                    lilDefaultValue = material.GetColor(propertyName);
                                }
                                if (propertyName == SHADER_KEY_POIYOMI_MainColorAdjustToggle)
                                {
                                    hasPoiyomiShader = true;
                                    poiyomiDefaultValue.Enable = material.GetInt(propertyName);
                                }
                                if (propertyName == SHADER_KEY_POIYOMI_Saturation)
                                {
                                    hasPoiyomiShader = true;
                                    poiyomiDefaultValue.Saturation = material.GetFloat(propertyName);
                                }
                            }
                        }
                    }
                    var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                    var targetShader = Parameters.TargetShader;
                    if (hasLilToon && targetShader.HasFlag(TargetShaders.lilToon))
                    {
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.r", AnimationCurve.Constant(0, 0, lilDefaultValue.r));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.g", AnimationCurve.Constant(0, 0, lilDefaultValue.g));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.b", AnimationCurve.Constant(0, 0, lilDefaultValue.b));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.a", AnimationCurve.Constant(0, 0, lilDefaultValue.a));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.r", AnimationCurve.Constant(0, 0, lilDefaultValue.r));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.g", AnimationCurve.Linear(0, 0, 1 / 60f, 2));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.b", AnimationCurve.Constant(0, 0, lilDefaultValue.b));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_LILTOON_MainHSVG}.a", AnimationCurve.Constant(0, 0, lilDefaultValue.a));
                    }
                    if (hasPoiyomiShader && targetShader.HasFlag(TargetShaders.Poiyomi))
                    {
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_MainColorAdjustToggle}", AnimationCurve.Constant(0, 0, poiyomiDefaultValue.Enable));
                        defaultAnim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_Saturation}", AnimationCurve.Constant(0, 0, poiyomiDefaultValue.Saturation));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_MainColorAdjustToggle}", AnimationCurve.Constant(0, 0, poiyomiDefaultValue.Enable));
                        anim.SetCurve(relativePath, type, $"material.{SHADER_KEY_POIYOMI_Saturation}", AnimationCurve.Linear(0, -1, 1 / 60f, 1));
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

            stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(-20, 100));

            fx.AddLayer(layer);

            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Saturation, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Saturation, saved = Parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
        }

        private void ConfigureResetParamerters(AnimatorController fx, ModularAvatarParameters parameters)
        {
            if (!Parameters.AddResetButton)
                return;

            AnimatorStateMachine stateMachine;
            var layer = new AnimatorControllerLayer()
            {
                name = "Reset",
                stateMachine = stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx),
                defaultWeight = 1,
            };
            var blank = new AnimationClip() { name = "Blank" }.HideInHierarchy().AddTo(fx);
            var off = new AnimatorState() { name = "Off", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(fx);
            var on = new AnimatorState() { name = "On", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(fx);

            var cond = new AnimatorCondition[] { new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = ParameterName_Reset } };

            var t = new AnimatorStateTransition() 
            {
                destinationState = on,
                duration = 0,
                hasExitTime = false,
                conditions = cond
            }.HideInHierarchy().AddTo(fx);

            off.AddTransition(t);

            cond[0].mode = AnimatorConditionMode.IfNot;
            t = new AnimatorStateTransition()
            {
                destinationState = off,
                duration = 0,
                hasExitTime = false,
                conditions = cond
            }.HideInHierarchy().AddTo(fx);

            on.AddTransition(t);

            var dr = on.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Value, value = Parameters.DefaultLightValue });
            if (Parameters.AllowSaturationControl)
            dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Saturation, value = 0.5f });

            stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

            fx.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);
            parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Reset, syncType = ParameterSyncType.Bool, localOnly = true, saved = false });

            fx.AddLayer(layer);
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

            if (Parameters.AddResetButton)
            {
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Reset",
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                    parameter = new VRCExpressionsMenu.Control.Parameter() { name = ParameterName_Reset }
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

        /*
         * Quouted from https://github.com/lilxyzw/lilToon/blob/2ef370dc444172787c075ec3a822438c2bee26cb/Assets/lilToon/Editor/lilEditorGUI.cs#L65
         *
         * Copyright (c) 2020-2023 lilxyzw
         * 
         * Full Licence: https://github.com/lilxyzw/lilToon/blob/master/LICENSE
        */
        private static void DrawWebButton(string text, string URL)
        {
            var position = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            var icon = EditorGUIUtility.IconContent("BuildSettings.Web.Small");
            icon.text = text;
            var style = new GUIStyle(EditorStyles.label) { padding = new RectOffset() };
            if (GUI.Button(position, icon, style))
            {
                Application.OpenURL(URL);
            }
        }
    }
}

#endif
