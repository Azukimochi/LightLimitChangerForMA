using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    public static class LightLimitGenerator
    {
        private const string SHADER_KEY_LILTOON_LightMinLimit = "_LightMinLimit";
        private const string SHADER_KEY_LILTOON_LightMaxLimit = "_LightMaxLimit";
        private const string SHADER_KEY_LILTOON_UNLIT = "_AsUnlit";
        private const string SHADER_KEY_LILTOON_MainHSVG = "_MainTexHSVG";
        private const string SHADER_KEY_LILTOON_COLOR = "_Color";
        private const string SHADER_KEY_LILTOON_COLOR2ND = "_Color2nd";
        private const string SHADER_KEY_LILTOON_COLOR3RD = "_Color3rd";

        private const string SHADER_KEY_LILTOON_BASECOLOR = "_Base Color";
        

        private const string SHADER_KEY_SUNAO_MinimumLight = "_MinimumLight";
        private const string SHADER_KEY_SUNAO_DirectionalLight = "_DirectionalLight";
        private const string SHADER_KEY_SUNAO_PointLight = "_PointLight";
        private const string SHADER_KEY_SUNAO_Unlit = "_Unlit";
        private const string SHADER_KEY_SUNAO_SHLight = "_SHLight";
        private const string SHADER_KEY_SUNAO_COLOR = "_Color";

        private const string SHADER_KEY_POIYOMI_LightingMinLightBrightness = "_LightingMinLightBrightness";
        private const string SHADER_KEY_POIYOMI_LightingCap = "_LightingCap";
        private const string SHADER_KEY_POIYOMI_MainColorAdjustToggle = "_MainColorAdjustToggle";
        private const string SHADER_KEY_POIYOMI_Saturation = "_Saturation";
        private const string SHADER_KEY_POIYOMI_COLOR = "_Color";

        private const string Poiyomi_Animated_Suffix = "Animated";
        private const string Poiyomi_Flag_IsAnimated = "1";

        private const string MATERIAL_ANIMATION_KEY_PREFIX = "material.";

        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ParameterName_Saturation = "LightLimitSaturation";
        private const string ParameterName_Unlit = "LightLimitUnlit";
        private const string ParameterName_ColorTemp = "LightLimitColorTemp";
        private const string ParameterName_Reset = "LightLimitReset";

        public static void Generate(VRCAvatarDescriptor avatar, LightLimitChangerSettings settings)
        {
            var fx = settings.FX;
            if (fx == null)
                fx = settings.FX = CreateTemporaryAsset();

            var obj = settings.gameObject;
            fx.parameters = Array.Empty<AnimatorControllerParameter>();
            var parameters = obj.UndoGetOrAddComponent<ModularAvatarParameters>(); 
            var menuInstaller = obj.UndoGetOrAddComponent<ModularAvatarMenuInstaller>();
            var mergeAnimator = obj.UndoGetOrAddComponent<ModularAvatarMergeAnimator>();
            parameters.parameters.Clear();
            menuInstaller.menuToAppend = CreateMenu(fx, settings.Parameters);
            mergeAnimator.deleteAttachedAnimator = true;
            mergeAnimator.animator = fx;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            fx.ClearSubAssets();
            fx.ClearLayers();

            ConfigureControls(avatar, settings);
            ConfigureResetParameters(settings);

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureControls(VRCAvatarDescriptor avatar, LightLimitChangerSettings settings)
        {
            var fx = settings.FX ;
            var parameters = settings.Parameters;
            var targets = new Dictionary<Renderer, Dictionary<Shaders, Material>>();

            AnimationClip baseColor = new AnimationClip() { name = "BaseColor" };
            var light = CreateAnimationClips("Light");
            var saturation = CreateAnimationClips("Saturation");
            var unlit = CreateAnimationClips("Unlit");
            var colorTemp = CreateAnimationClips("ColorTemp");

            light.AddTo(fx);

            if (parameters.AllowColorTempControl)
            {
                colorTemp.AddTo(fx);
            }
            if (parameters.AllowSaturationControl)
            {
                baseColor.AddTo(fx);
                saturation.AddTo(fx);
            }
            if (parameters.AllowUnlitControl)
            {
                unlit.AddTo(fx);
            }

            if (BuildManager.IsRunning)
            {
                var objectMapper = ObjectMapper.Create();
                var components = avatar.GetComponentsInChildren<Component>().Where(x => !(x is Transform)).Select(x => new SerializedObject(x)).ToArray();
                objectMapper.MapObject<Material>(components, x =>
                {
                    x = x.Clone().AddTo(fx);
                    MaterialNormalizer.Normalize(x);
                    return x;
                });

                var animatorMapper = new AnimatorControllerMapper(objectMapper.MappedObjects, fx);
                bool needAnimatorCloniong = false;

                objectMapper.MapObject<RuntimeAnimatorController>(components, x =>
                {
                    foreach (var anim in x.animationClips)
                    {
                        var binds = AnimationUtility.GetObjectReferenceCurveBindings(anim);
                        foreach (var bind in binds)
                        {
                            if (bind.type.BaseType == typeof(Renderer))
                            {
                                var renderer = GameObject.Find(bind.path)?.GetComponent(bind.type) as Renderer;
                                if (renderer != null)
                                {
                                    var curves = AnimationUtility.GetObjectReferenceCurve(anim, bind);
                                    foreach (var curve in curves)
                                    {
                                        var material = curve.value as Material;
                                        if ((GetShaderType(material?.shader) & parameters.TargetShader) != 0)
                                        {
                                            needAnimatorCloniong = true;
                                            goto EndSearchAniamtions;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    EndSearchAniamtions:
                    if (needAnimatorCloniong)
                    {
                        return animatorMapper.MapController(x);
                    }
                    else
                    {
                        return null;
                    }
                });
            }

            foreach(var renderer in avatar.GetComponentsInChildren<Renderer>())
            {
                var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                var type = renderer.GetType();

                var (min, max, color, color2nd, color3rd, hsvg, dir, point, sh, sat) =
                    (
                        0f,
                        1f,
                        Color.white,
                        Color.white,
                        Color.white,
                        new Vector4(0, 1, 1, 1),
                        1f,
                        1f,
                        1f,
                        0f
                    );

                if (parameters.TargetShader.HasFlag(Shaders.lilToon))
                {
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMinLimit}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMaxLimit}", Utils.Animation.Constant(max));

                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMinLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));
                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMaxLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));

                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.r", Utils.Animation.Constant(color.r));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.g", Utils.Animation.Constant(color.g));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.b", Utils.Animation.Constant(color.b));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.a", Utils.Animation.Constant(color.a));

                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.r", Utils.Animation.Constant(color2nd.r));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.g", Utils.Animation.Constant(color2nd.g));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.b", Utils.Animation.Constant(color2nd.b));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.a", Utils.Animation.Constant(color2nd.a));

                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.r", Utils.Animation.Constant(color3rd.r));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.g", Utils.Animation.Constant(color3rd.g));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.b", Utils.Animation.Constant(color3rd.b));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.a", Utils.Animation.Constant(color3rd.a));

                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.r", Utils.Animation.Linear(color.r * 0.6f, color.r, color.r));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g * 0.8f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.b", Utils.Animation.Linear(color.b, color.b, color.b * 0.6f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.a", Utils.Animation.Constant(color.a));

                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.r", Utils.Animation.Linear(color2nd.r * 0.6f, color2nd.r, color2nd.r));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.g", Utils.Animation.Linear(color2nd.g * 0.95f, color2nd.g, color2nd.g * 0.8f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.b", Utils.Animation.Linear(color2nd.b, color2nd.b, color2nd.b * 0.6f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.a", Utils.Animation.Constant(color2nd.a));

                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.r", Utils.Animation.Linear(color3rd.r * 0.6f, color3rd.r, color3rd.r));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.g", Utils.Animation.Linear(color3rd.g * 0.95f, color3rd.g, color3rd.g * 0.8f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.b", Utils.Animation.Linear(color3rd.b, color3rd.b, color3rd.b * 0.6f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.a", Utils.Animation.Constant(color3rd.a));

                    baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.x", Utils.Animation.Constant(hsvg.x));
                    baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.y", Utils.Animation.Constant(hsvg.y));
                    baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.z", Utils.Animation.Constant(hsvg.z));
                    baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.w", Utils.Animation.Constant(hsvg.w));

                    saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.y", Utils.Animation.Linear(0, 2));

                    unlit.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_UNLIT}", Utils.Animation.Constant(0));
                    unlit.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_UNLIT}", Utils.Animation.Linear(0.0f, 1.0f));
                }

                if (parameters.TargetShader.HasFlag(Shaders.Sunao))
                {
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_MinimumLight}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_DirectionalLight}", Utils.Animation.Constant(dir));
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_PointLight}", Utils.Animation.Constant(point));
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_SHLight}", Utils.Animation.Constant(sh));

                    var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_MinimumLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_DirectionalLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_PointLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_SHLight}", curve);


                    unlit.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_Unlit}", Utils.Animation.Constant(0));
                    unlit.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_Unlit}", Utils.Animation.Linear(0.0f, 1.0f));

                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.r", Utils.Animation.Constant(color.r));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.g", Utils.Animation.Constant(color.g));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.b", Utils.Animation.Constant(color.b));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.a", Utils.Animation.Constant(color.a));

                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.r", Utils.Animation.Linear(color.r * 0.6f, color.r, color.r));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g * 0.8f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.b", Utils.Animation.Linear(color.b, color.b, color.b * 0.6f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.a", Utils.Animation.Constant(color.a));
                }

                if (parameters.TargetShader.HasFlag(Shaders.Poiyomi))
                {
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingCap}", Utils.Animation.Constant(max));

                    var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", curve);
                    light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingCap}", curve);

                    saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_Saturation}", Utils.Animation.Constant(sat));
                    saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_Saturation}", Utils.Animation.Linear(-1, 1));

                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.r", Utils.Animation.Constant(color.r));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.g", Utils.Animation.Constant(color.g));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.b", Utils.Animation.Constant(color.b));
                    colorTemp.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.a", Utils.Animation.Constant(color.a));

                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.r", Utils.Animation.Linear(color.r * 0.6f, color.r, color.r));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g * 0.8f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.b", Utils.Animation.Linear(color.b, color.b, color.b * 0.6f));
                    colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.a", Utils.Animation.Constant(color.a));
                }
            }

            AddLayer(fx, "Light", light.Default, light.Control, ParameterName_Value);

            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = parameters.DefaultLightValue, type = AnimatorControllerParameterType.Float });

            var param = settings.gameObject.GetOrAddComponent<ModularAvatarParameters>();

            param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = parameters.IsValueSave, defaultValue = parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });
            param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Value, saved = parameters.IsValueSave, defaultValue = parameters.DefaultLightValue, syncType = ParameterSyncType.Float });

            if (parameters.AllowColorTempControl || parameters.AllowSaturationControl)
            {
                AddLayer(fx, "BaseColor", baseColor);
            }
            if (parameters.AllowSaturationControl)
            {
                AddLayer(fx, "Saturation", saturation.Default, saturation.Control, ParameterName_Saturation);

                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Saturation, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Saturation, saved = parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
            }

            if(parameters.AllowUnlitControl)
            {
                AddLayer(fx, "Unlit", unlit.Default, unlit.Control, ParameterName_Unlit);

                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Unlit, defaultFloat = 0.0f, type = AnimatorControllerParameterType.Float });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Unlit, saved = parameters.IsValueSave, defaultValue = 0.0f, syncType = ParameterSyncType.Float });
            }

            if (parameters.AllowColorTempControl)
            {
                AddLayer(fx, "ColorTemp", colorTemp.Default, colorTemp.Control, ParameterName_ColorTemp);

                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_ColorTemp, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_ColorTemp, saved = parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
            }
        }

        private static void ConfigureResetParameters(LightLimitChangerSettings settings)
        {
            if (!settings.Parameters.AddResetButton)
                return;

            var fx = settings.FX;

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
            dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Value, value = settings.Parameters.DefaultLightValue });
            if (settings.Parameters.AllowColorTempControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_ColorTemp, value = 0.5f });
            if (settings.Parameters.AllowSaturationControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Saturation, value = 0.5f });
            if (settings.Parameters.AllowUnlitControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Unlit, value = 0.0f });
            stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

            fx.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);
            settings.gameObject.GetOrAddComponent<ModularAvatarParameters>().parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Reset, syncType = ParameterSyncType.Bool, localOnly = true, saved = false });

            fx.AddLayer(layer);
        }

        internal static Shaders GetShaderType(Shader shader)
        {
            Shaders result = 0;
            if (shader == null)
                return result;
            string name = shader.name;
            if (name.IndexOf("lilToon", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = Shaders.lilToon;
            }
            else if (name.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = Shaders.Poiyomi;
            }
            else if (name.IndexOf("Sunao", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = Shaders.Sunao;
            }

            if (result == 0)
            {
                int count = shader.GetPropertyCount();
                for (int i = 0; i < count; i++)
                {
                    var propertyName = shader.GetPropertyName(i);
                    switch (propertyName)
                    {
                        case SHADER_KEY_LILTOON_LightMinLimit:
                        case SHADER_KEY_LILTOON_LightMaxLimit:
                        case SHADER_KEY_LILTOON_MainHSVG:
                        case SHADER_KEY_LILTOON_UNLIT:
                            result = Shaders.lilToon;
                            break;

                        case SHADER_KEY_SUNAO_MinimumLight:
                        case SHADER_KEY_SUNAO_DirectionalLight:
                        case SHADER_KEY_SUNAO_PointLight:
                        case SHADER_KEY_SUNAO_SHLight:
                            result = Shaders.Sunao;
                            break;

                        case SHADER_KEY_POIYOMI_LightingMinLightBrightness:
                        case SHADER_KEY_POIYOMI_LightingCap:
                        case SHADER_KEY_POIYOMI_MainColorAdjustToggle:
                        case SHADER_KEY_POIYOMI_Saturation:
                            result = Shaders.Poiyomi;
                            break;
                    }
                }
            }

            return result;
        }

        private static void AddLayer(AnimatorController fx, string name, AnimationClip @default)
        {
            var layer = new AnimatorControllerLayer() { name = name, defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = @default }.HideInHierarchy().AddTo(fx);

            stateMachine.AddState(defaultState, stateMachine.entryPosition + new Vector3(-20, 50));
            fx.AddLayer(layer);
        }

        private static void AddLayer(AnimatorController fx, string name, AnimationClip @default, AnimationClip control, string parameterName)
        {
            var layer = new AnimatorControllerLayer() { name = name, defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = @default }.HideInHierarchy().AddTo(fx);
            var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = control, timeParameterActive = true, timeParameter = parameterName }.HideInHierarchy().AddTo(fx);

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
        }

        private static VRCExpressionsMenu CreateMenu(AnimatorController fx, LightLimitChangerParameters parameters)
        {
            var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(fx);
            mainMenu.name = "Main Menu";
            mainMenu.controls = new List<VRCExpressionsMenu.Control>
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
            };

            if (parameters.AllowColorTempControl)
            {
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "ColorTemperature",
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    subParameters = new VRCExpressionsMenu.Control.Parameter[]
                    {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_ColorTemp
                        }
                    },
                });
            }
            if (parameters.AllowSaturationControl)
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
            if (parameters.AllowUnlitControl)
            {
                mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Unlit",
                    type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                    subParameters = new VRCExpressionsMenu.Control.Parameter[]
                    {
                        new VRCExpressionsMenu.Control.Parameter
                        {
                            name = ParameterName_Unlit
                        }
                    },
                });
            }


            if (parameters.AddResetButton)
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

        private static AnimatorController CreateTemporaryAsset()
        {
            var fx = new AnimatorController() { name = GUID.Generate().ToString() };
            AssetDatabase.CreateAsset(fx, System.IO.Path.Combine(Utils.GetGeneratedAssetsFolder(), $"{fx.name}.controller"));
            AssetDatabase.SaveAssets();
            return fx;
        }

        private static (AnimationClip Default, AnimationClip Control) CreateAnimationClips(string name)
            => (new AnimationClip() { name = $"Default {name}" }, new AnimationClip() { name = $"Change {name}" });

        private static (T, T) AddTo<T>(this (T, T) tuple, Object asset) where T : Object
            => (tuple.Item1.AddTo(asset), tuple.Item2.AddTo(asset));

        internal struct ObjectMapper
        {
            public Dictionary<Object, Object> MappedObjects { get; }

            private ObjectMapper(Dictionary<Object, Object> map) => MappedObjects = map;

            public static ObjectMapper Create() => new ObjectMapper(new Dictionary<Object, Object>());

            public void MapObject<T>(SerializedObject[] objects, Func<T, T> factory) where T : Object
            {
                foreach (var serializedObject in objects)
                {
                    bool enterChildren = true;
                    var p = serializedObject.GetIterator();
                    while (p.Next(enterChildren))
                    {
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (p.objectReferenceValue is T obj)
                            {
                                if (!MappedObjects.TryGetValue(obj, out var mapped))
                                {
                                    mapped = factory(obj);
                                    if (mapped == null)
                                        continue;
                                    MappedObjects.Add(obj, mapped);
                                }
                                p.objectReferenceValue = mapped;
                            }
                        }

                        switch (p.propertyType)
                        {
                            case SerializedPropertyType.String:
                            case SerializedPropertyType.Integer:
                            case SerializedPropertyType.Boolean:
                            case SerializedPropertyType.Float:
                            case SerializedPropertyType.Color:
                            case SerializedPropertyType.ObjectReference:
                            case SerializedPropertyType.LayerMask:
                            case SerializedPropertyType.Enum:
                            case SerializedPropertyType.Vector2:
                            case SerializedPropertyType.Vector3:
                            case SerializedPropertyType.Vector4:
                            case SerializedPropertyType.Rect:
                            case SerializedPropertyType.ArraySize:
                            case SerializedPropertyType.Character:
                            case SerializedPropertyType.AnimationCurve:
                            case SerializedPropertyType.Bounds:
                            case SerializedPropertyType.Gradient:
                            case SerializedPropertyType.Quaternion:
                            case SerializedPropertyType.FixedBufferSize:
                            case SerializedPropertyType.Vector2Int:
                            case SerializedPropertyType.Vector3Int:
                            case SerializedPropertyType.RectInt:
                            case SerializedPropertyType.BoundsInt:
                                enterChildren = false;
                                break;
                            case SerializedPropertyType.Generic:
                            case SerializedPropertyType.ExposedReference:
                            case SerializedPropertyType.ManagedReference:
                            default:
                                enterChildren = true;
                                break;
                        }
                    }
                }
            }
        }

        // https://github.com/anatawa12/AvatarOptimizer/blob/f31d71e318a857b4f4d7db600f043c3ba5d26918/Editor/Processors/ApplyObjectMapping.cs#L136-L303
        // Originally under MIT License
        // Copyright (c) 2022 anatawa12
        internal class AnimatorControllerMapper
        {
            private readonly Dictionary<Object, Object> _mapping;
            private readonly Dictionary<Object, Object> _cache = new Dictionary<Object, Object>();
            private readonly Object _rootArtifact;
            private bool _mapped = false;


            public AnimatorControllerMapper(Dictionary<Object, Object> mapping, Object rootArtifact)
            {
                _rootArtifact = rootArtifact;
                _mapping = mapping;
            }

            public RuntimeAnimatorController MapController(RuntimeAnimatorController controller)
            {
                if (controller is AnimatorController animatorController)
                {
                    return MapAnimatorController(animatorController);
                }
                else if (controller is AnimatorOverrideController overrideController)
                {
                    return MapAnimatorOverrideController(overrideController);
                }

                throw new NotSupportedException($"Type \"{controller.GetType()}\" is not supported");
            }


            public AnimatorController MapAnimatorController(AnimatorController controller)
            {
                if (_cache.TryGetValue(controller, out var cached)) return (AnimatorController)cached;
                _mapped = false;
                var newController = new AnimatorController
                {
                    parameters = controller.parameters,
                    layers = controller.layers.Select(MapAnimatorControllerLayer).ToArray()
                };
                if (!_mapped) newController = null;
                _cache[controller] = newController;
                return newController?.AddTo(_rootArtifact);
            }

            public AnimatorOverrideController MapAnimatorOverrideController(AnimatorOverrideController controller)
            {
                if (_cache.TryGetValue(controller, out var cached)) return (AnimatorOverrideController)cached;
                _mapped = false;

                controller = controller.Clone();
                var list = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
                controller.GetOverrides(list);
                for (int i = 0; i < list.Count; i++)
                {
                    var x = list[i];
                    var clip = x.Value;

                    var newClip = new AnimationClip
                    {
                        name = $"remapped {clip.name}"
                    };

                    bool changed = false;

                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    {
                        var curves = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                        for (int i2 = 0; i2 < curves.Length; i2++)
                        {
                            if (curves[i2].value is Material material && _mapping.TryGetValue(material, out var mapped))
                            {
                                curves[i2].value = mapped;
                                _mapped = true;
                                changed = true;
                            }
                        }
                        AnimationUtility.SetObjectReferenceCurve(newClip, binding, curves);
                    }

                    if (changed)
                    {
                        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                        {
                            newClip.SetCurve(binding.path, binding.type, binding.propertyName, AnimationUtility.GetEditorCurve(clip, binding));
                        }

                        newClip.wrapMode = clip.wrapMode;
                        newClip.legacy = clip.legacy;
                        newClip.frameRate = clip.frameRate;
                        newClip.localBounds = clip.localBounds;
                        AnimationUtility.SetAnimationClipSettings(newClip, AnimationUtility.GetAnimationClipSettings(clip));

                        list[i] = new KeyValuePair<AnimationClip, AnimationClip>(x.Key, newClip.AddTo(_rootArtifact));
                    }
                }

                if (!_mapped)
                {
                    controller = null; 
                }
                else
                {
                    controller.ApplyOverrides(list);
                }
                _cache[controller] = controller;
                return controller?.AddTo(_rootArtifact);
            }

            private AnimatorControllerLayer MapAnimatorControllerLayer(AnimatorControllerLayer layer) =>
                new AnimatorControllerLayer
                {
                    name = layer.name,
                    avatarMask = layer.avatarMask,
                    blendingMode = layer.blendingMode,
                    defaultWeight = layer.defaultWeight,
                    syncedLayerIndex = layer.syncedLayerIndex,
                    syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming,
                    iKPass = layer.iKPass,
                    stateMachine = MapStateMachine(layer.stateMachine),
                };


            private AnimatorStateMachine MapStateMachine(AnimatorStateMachine stateMachine) =>
                DeepClone(stateMachine, CustomClone);


            // https://github.com/bdunderscore/modular-avatar/blob/db49e2e210bc070671af963ff89df853ae4514a5/Packages/nadena.dev.modular-avatar/Editor/AnimatorMerger.cs#L199-L241
            // Originally under MIT License
            // Copyright (c) 2022 bd_
            private Object CustomClone(Object o)
            {
                if (o is AnimationClip clip)
                {
                    var newClip = new AnimationClip();
                    newClip.name = $"remapped {clip.name}";
                    bool changed = false;

                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    {
                        var curves = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                        for (int i = 0; i < curves.Length; i++)
                        {
                            var x = curves[i];
                            if (x.value is Material material && _mapping.TryGetValue(material, out var mapped))
                            {
                                x.value = mapped;
                                _mapped = true;
                                changed = true;
                            }
                            curves[i] = x;
                        }
                        AnimationUtility.SetObjectReferenceCurve(newClip, binding, curves);
                    }

                    if (!changed)
                        return null;

                    newClip.AddTo(_rootArtifact);

                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        newClip.SetCurve(binding.path, binding.type, binding.propertyName, AnimationUtility.GetEditorCurve(clip, binding));
                    }

                    newClip.wrapMode = clip.wrapMode;
                    newClip.legacy = clip.legacy;
                    newClip.frameRate = clip.frameRate;
                    newClip.localBounds = clip.localBounds;
                    AnimationUtility.SetAnimationClipSettings(newClip, AnimationUtility.GetAnimationClipSettings(clip));

                    return newClip;
                }
                else
                {
                    return null;
                }
            }


            // https://github.com/bdunderscore/modular-avatar/blob/db49e2e210bc070671af963ff89df853ae4514a5/Packages/nadena.dev.modular-avatar/Editor/AnimatorMerger.cs#LL242-L340C10
            // Originally under MIT License
            // Copyright (c) 2022 bd_
            private T DeepClone<T>(T original, Func<Object, Object> visitor) where T : Object
            {
                if (original == null) return null;


                // We want to avoid trying to copy assets not part of the animation system (eg - textures, meshes,
                // MonoScripts...), so check for the types we care about here
                switch (original)
                {
                    // Any object referenced by an animator that we intend to mutate needs to be listed here.
                    case Motion _:
                    case AnimatorController _:
                    case AnimatorState _:
                    case AnimatorStateMachine _:
                    case AnimatorTransitionBase _:
                    case StateMachineBehaviour _:
                        break; // We want to clone these types


                    // Leave textures, materials, and script definitions alone
                    case Texture _:
                    case MonoScript _:
                    case Material _:
                        return original;


                    // Also avoid copying unknown scriptable objects.
                    // This ensures compatibility with e.g. avatar remote, which stores state information in a state
                    // behaviour referencing a custom ScriptableObject
                    case ScriptableObject _:
                        return original;


                    default:
                        throw new Exception($"Unknown type referenced from animator: {original.GetType()}");
                }
                if (_cache.TryGetValue(original, out var cached)) return (T)cached;


                var obj = visitor(original);
                if (obj != null)
                {
                    _cache[original] = obj;
                    return (T)obj;
                }


                var ctor = original.GetType().GetConstructor(Type.EmptyTypes);
                if (ctor == null || original is ScriptableObject)
                {
                    obj = Object.Instantiate(original);
                }
                else
                {
                    obj = (T)ctor.Invoke(Array.Empty<object>());
                    EditorUtility.CopySerialized(original, obj);
                }


                _cache[original] = obj.HideInHierarchy().AddTo(_rootArtifact);


                SerializedObject so = new SerializedObject(obj);
                SerializedProperty prop = so.GetIterator();


                bool enterChildren = true;
                while (prop.Next(enterChildren))
                {
                    enterChildren = true;
                    switch (prop.propertyType)
                    {
                        case SerializedPropertyType.ObjectReference:
                            prop.objectReferenceValue = DeepClone(prop.objectReferenceValue, visitor);
                            break;
                        // Iterating strings can get super slow...
                        case SerializedPropertyType.String:
                            enterChildren = false;
                            break;
                    }
                }


                so.ApplyModifiedPropertiesWithoutUndo();


                return (T)obj;
            }
        }
    }
}
