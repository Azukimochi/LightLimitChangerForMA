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
            var targets = new Dictionary<Shaders, List<(Renderer Renderer, Material Material)>>();

            Dictionary<Material, Material> materialMapping = null;

            foreach (var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if ((renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && (!settings.Parameters.ExcludeEditorOnly || renderer.tag != "EditorOnly"))
                {
                    var materials = renderer.sharedMaterials;
                    bool materialsHasChanged = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material material = materials[i];
                        if (material != null)
                        {
                            var shaderType = GetShaderType(material?.shader);
                            if ((shaderType & parameters.TargetShader) != 0)
                            {
                                if (BuildManager.IsRunning)
                                {
                                    if (!(materialMapping ?? (materialMapping = new Dictionary<Material, Material>())).TryGetValue(material, out var clone))
                                    {
                                        clone = material.Clone().AddTo(fx);
                                        materialMapping.Add(material, clone);
                                    }

                                    material = materials[i] = clone;
                                    materialsHasChanged = true;
                                }

                                var list = targets.GetOrAdd(shaderType, _ => new List<(Renderer Renderer, Material Material)>());
                                list.Add((renderer, material));
                            }
                        }
                    }
                    if (materialsHasChanged)
                    {
                        renderer.sharedMaterials = materials;
                    }
                }
            }

            AnimationClip baseColor = new AnimationClip() { name = "BaseColor" };
            (AnimationClip Default, AnimationClip Control) light = (new AnimationClip() { name = "Default Light" }, new AnimationClip() { name = "Change Light" });
            (AnimationClip Default, AnimationClip Control) saturation = (new AnimationClip() { name = "Default Saturation" }, new AnimationClip() { name = "Change Saturation" });
            (AnimationClip Default, AnimationClip Control) unlit = (new AnimationClip() { name = "Default Unlit" }, new AnimationClip() { name = "Change Unlit" });
            (AnimationClip Default, AnimationClip Control) colorTemp = (new AnimationClip() { name = "Default ColorTemp" }, new AnimationClip() { name = "Change ColorTemp" });

            light.Default.AddTo(fx);
            light.Control.AddTo(fx);

            if (parameters.AllowColorTempControl)
            {
                colorTemp.Default.AddTo(fx);
                colorTemp.Control.AddTo(fx);
            }
            if (parameters.AllowSaturationControl)
            {
                baseColor.AddTo(fx);
                saturation.Default.AddTo(fx);
                saturation.Control.AddTo(fx);
            }
            if (parameters.AllowUnlitControl)
            {
                unlit.Default.AddTo(fx);
                unlit.Control.AddTo(fx);
            }

            foreach (var target in targets)
            {
                foreach (var (renderer, material) in target.Value)
                {
                    var key = target.Key & parameters.TargetShader;
                    if (key == 0)
                        continue;

                    var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                    var type = renderer.GetType();

                    if (key.HasFlag(Shaders.lilToon))
                    {
                        var (min, max, color, color2nd, color3rd, sat) =
                        (
                            material.GetFloat(SHADER_KEY_LILTOON_LightMinLimit),
                            material.GetFloat(SHADER_KEY_LILTOON_LightMaxLimit),
                            material.GetColor(SHADER_KEY_LILTOON_COLOR),
                            material.GetColor(SHADER_KEY_LILTOON_COLOR2ND),
                            material.GetColor(SHADER_KEY_LILTOON_COLOR3RD),
                            material.GetVector(SHADER_KEY_LILTOON_MainHSVG)
                        );

                        if (parameters.OverwriteDefaultLightMinMax)
                            (min, max) = (parameters.MinLightValue, parameters.MaxLightValue);

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
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g *0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.b", Utils.Animation.Linear(color.b, color.b,color.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.a", Utils.Animation.Constant(color.a));

                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.r", Utils.Animation.Linear(color2nd.r * 0.6f, color2nd.r, color2nd.r));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.g", Utils.Animation.Linear(color2nd.g * 0.95f, color2nd.g, color2nd.g *0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.b", Utils.Animation.Linear(color2nd.b, color2nd.b,color2nd.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR2ND}.a", Utils.Animation.Constant(color2nd.a));
                        
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.r", Utils.Animation.Linear(color3rd.r * 0.6f, color3rd.r, color3rd.r));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.g", Utils.Animation.Linear(color3rd.g * 0.95f, color3rd.g, color3rd.g *0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.b", Utils.Animation.Linear(color3rd.b, color3rd.b,color3rd.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR3RD}.a", Utils.Animation.Constant(color3rd.a));
                        
                        //colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.r", Utils.Animation.Linear(0.6f, 1, 1));
                        //colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.g", Utils.Animation.Linear(0.95f, 1, 0.8f));
                        //colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.b", Utils.Animation.Linear(1, 1,0.6f));
                        //colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_COLOR}.a", Utils.Animation.Constant(1));

                        baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.x", Utils.Animation.Constant(sat.x));
                        baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.y", Utils.Animation.Constant(sat.y));
                        baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.z", Utils.Animation.Constant(sat.z));
                        baseColor.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.w", Utils.Animation.Constant(sat.w));
                        
                        
                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.y", Utils.Animation.Linear(0, 2));

                        unlit.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_UNLIT}", Utils.Animation.Constant(0));
                        unlit.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_UNLIT}", Utils.Animation.Linear(0.0f, 1.0f));
                    }
                    if (key.HasFlag(Shaders.Sunao))
                    {
                        var (min, dir, point, sh, color) =
                        (
                            material.GetFloat(SHADER_KEY_SUNAO_MinimumLight),
                            material.GetFloat(SHADER_KEY_SUNAO_DirectionalLight),
                            material.GetFloat(SHADER_KEY_SUNAO_PointLight),
                            material.GetFloat(SHADER_KEY_SUNAO_SHLight),
                            material.GetColor(SHADER_KEY_SUNAO_COLOR)
                        );

                        if (parameters.OverwriteDefaultLightMinMax)
                            min = parameters.MinLightValue;

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
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g *0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.b", Utils.Animation.Linear(color.b, color.b,color.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_COLOR}.a", Utils.Animation.Constant(color.a));

                    }
                    if (key.HasFlag(Shaders.Poiyomi))
                    {
                        if (parameters.AllowOverridePoiyomiAnimTag && BuildManager.IsRunning)
                        {
                            Debug.Log($"{material.name} ({AssetDatabase.GetAssetPath(material)})");

                            TrySetAnimated(SHADER_KEY_POIYOMI_LightingCap);
                            TrySetAnimated(SHADER_KEY_POIYOMI_LightingMinLightBrightness);

                            if (parameters.AllowColorTempControl)
                            {
                                TrySetAnimated(SHADER_KEY_POIYOMI_COLOR);
                            }
                            if (parameters.AllowSaturationControl)
                            {
                                TrySetAnimated(SHADER_KEY_POIYOMI_Saturation);
                            }

                            void TrySetAnimated(string property)
                            {
                                var name = $"{property}{Poiyomi_Animated_Suffix}";
                                material.SetOverrideTag(name, Poiyomi_Flag_IsAnimated);
                            }
                        }

                        if (parameters.AllowSaturationControl)
                        {
                            material.SetFloat("_MainColorAdjustToggle", 1);
                            material.EnableKeyword($"{"_MainColorAdjustToggle".ToUpperInvariant()}_ON");
                        }

                        var (min, max, sat, color) =
                        (
                            material.GetFloat(SHADER_KEY_POIYOMI_LightingMinLightBrightness),
                            material.GetFloat(SHADER_KEY_POIYOMI_LightingCap),
                            material.GetFloat(SHADER_KEY_POIYOMI_Saturation),
                            material.GetColor(SHADER_KEY_POIYOMI_COLOR)
                        );

                        if (parameters.OverwriteDefaultLightMinMax)
                            (min, max) = (parameters.MinLightValue, parameters.MaxLightValue);

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
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g *0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.b", Utils.Animation.Linear(color.b, color.b,color.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_COLOR}.a", Utils.Animation.Constant(color.a));
                        
                    }
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

        private static Shaders GetShaderType(Shader shader)
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
    }
}
