using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private const string SHADER_KEY_LILTOON_MainHSVG = "_MainTexHSVG";

        private const string SHADER_KEY_SUNAO_MinimumLight = "_MinimumLight";
        private const string SHADER_KEY_SUNAO_DirectionalLight = "_DirectionalLight";
        private const string SHADER_KEY_SUNAO_PointLight = "_PointLight";
        private const string SHADER_KEY_SUNAO_SHLight = "_SHLight";

        private const string SHADER_KEY_POIYOMI_LightingMinLightBrightness = "_LightingMinLightBrightness";
        private const string SHADER_KEY_POIYOMI_LightingCap = "_LightingCap";
        private const string SHADER_KEY_POIYOMI_MainColorAdjustToggle = "_MainColorAdjustToggle";
        private const string SHADER_KEY_POIYOMI_Saturation = "_Saturation";

        private const string MATERIAL_ANIMATION_KEY_PREFIX = "material.";

        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ParameterName_Saturation = "LightLimitSaturation";
        private const string ParameterName_Reset = "LightLimitReset";

        public static void Generate(VRCAvatarDescriptor avatar, LightLimitChangerSettings settings)
        {
            var fx = settings.FX;
            var obj = settings.gameObject;
            fx.parameters = Array.Empty<AnimatorControllerParameter>();
            var parameters = obj.GetOrAddComponent<ModularAvatarParameters>();
            var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();
            var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
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
            ConfigureResetParamerters(settings);

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureControls(VRCAvatarDescriptor avatar, LightLimitChangerSettings settings)
        {
            var fx = settings.FX;
            var parameters = settings.Parameters;
            var enumerable = avatar.GetComponentsInChildren<Renderer>(true).Where(x => x is MeshRenderer || x is SkinnedMeshRenderer).Select(renderer =>
            {
                var materials = renderer.sharedMaterials.Select(material =>
                {
                    Shaders shaders = 0;
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
                                case SHADER_KEY_LILTOON_LightMaxLimit:
                                case SHADER_KEY_LILTOON_MainHSVG:
                                    shaders |= Shaders.lilToon;
                                    continue;

                                case SHADER_KEY_SUNAO_MinimumLight:
                                case SHADER_KEY_SUNAO_DirectionalLight:
                                case SHADER_KEY_SUNAO_PointLight:
                                case SHADER_KEY_SUNAO_SHLight:
                                    shaders |= Shaders.Sunao;
                                    continue;

                                case SHADER_KEY_POIYOMI_LightingMinLightBrightness:
                                case SHADER_KEY_POIYOMI_LightingCap:
                                case SHADER_KEY_POIYOMI_MainColorAdjustToggle:
                                case SHADER_KEY_POIYOMI_Saturation:
                                    shaders |= Shaders.Poiyomi;
                                    continue;
                            }
                        }
                    }
                    return (material, shaders);
                }).Where(x => x.shaders != 0);

                return (renderer, materials);
            }).SelectMany(x => x.materials.Select(y => (Renderer: x.renderer, Material: y.material, Shaders: y.shaders))).GroupBy(x => x.Shaders, y => (y.Renderer, y.Material));

            (AnimationClip Default, AnimationClip Control) light = (new AnimationClip() { name = "Default Light" }, new AnimationClip() { name = "Change Light" });
            (AnimationClip Default, AnimationClip Control) saturation = (new AnimationClip() { name = "Default Saturation" }, new AnimationClip() { name = "Change Saturation" });

            light.Default.AddTo(fx);
            light.Control.AddTo(fx);

            if (parameters.AllowSaturationControl)
            {
                saturation.Default.AddTo(fx);
                saturation.Control.AddTo(fx);
            }

            foreach (var group in enumerable)
            {
                foreach (var (renderer, material) in group)
                {
                    var key = group.Key & parameters.TargetShader;
                    if (key == 0)
                        continue;

                    var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                    var type = renderer.GetType();

                    if (key.HasFlag(Shaders.lilToon))
                    {
                        var (min, max, sat) =
                        (
                            material.GetFloat(SHADER_KEY_LILTOON_LightMinLimit),
                            material.GetFloat(SHADER_KEY_LILTOON_LightMaxLimit),
                            material.GetColor(SHADER_KEY_LILTOON_MainHSVG)
                        );

                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMinLimit}", Utils.Animation.Constant(min));
                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMaxLimit}", Utils.Animation.Constant(max));

                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMinLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));
                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_LightMaxLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));

                        saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.r", Utils.Animation.Constant(sat.r));
                        saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.g", Utils.Animation.Constant(sat.g));
                        saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.b", Utils.Animation.Constant(sat.b));
                        saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.a", Utils.Animation.Constant(sat.a));

                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.r", Utils.Animation.Constant(sat.r));
                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.g", Utils.Animation.Linear(0, 2));
                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.b", Utils.Animation.Constant(sat.b));
                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_LILTOON_MainHSVG}.a", Utils.Animation.Constant(sat.a));
                    }
                    if (key.HasFlag(Shaders.Sunao))
                    {
                        var (min, dir, point, sh) =
                        (
                            material.GetFloat(SHADER_KEY_SUNAO_MinimumLight),
                            material.GetFloat(SHADER_KEY_SUNAO_DirectionalLight),
                            material.GetFloat(SHADER_KEY_SUNAO_PointLight),
                            material.GetFloat(SHADER_KEY_SUNAO_SHLight)
                        );

                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_MinimumLight}", Utils.Animation.Constant(min));
                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_DirectionalLight}", Utils.Animation.Constant(dir));
                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_PointLight}", Utils.Animation.Constant(point));
                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_SHLight}", Utils.Animation.Constant(sh));

                        var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_MinimumLight}", curve);
                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_DirectionalLight}", curve);
                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_PointLight}", curve);
                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_SUNAO_SHLight}", curve);
                    }
                    if (key.HasFlag(Shaders.Poiyomi))
                    {
                        var (min, max, sat) =
                        (
                            material.GetFloat(SHADER_KEY_POIYOMI_LightingMinLightBrightness),
                            material.GetFloat(SHADER_KEY_POIYOMI_MainColorAdjustToggle),
                            material.GetFloat(SHADER_KEY_POIYOMI_Saturation)
                        );

                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", Utils.Animation.Constant(min));
                        light.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingCap}", Utils.Animation.Constant(max));

                        var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingMinLightBrightness}", curve);
                        light.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_LightingCap}", curve);

                        saturation.Default.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_Saturation}", Utils.Animation.Constant(sat));
                        saturation.Control.SetCurve(relativePath, type, $"{MATERIAL_ANIMATION_KEY_PREFIX}{SHADER_KEY_POIYOMI_Saturation}", Utils.Animation.Linear(-1, 1));
                    }
                }
            }

            AddLayer(fx, "Light", light.Default, light.Control);

            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
            fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = parameters.DefaultLightValue, type = AnimatorControllerParameterType.Float });

            var param = settings.gameObject.GetOrAddComponent<ModularAvatarParameters>();

            param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = parameters.IsValueSave, defaultValue = parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });
            param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Value, saved = parameters.IsValueSave, defaultValue = parameters.DefaultLightValue, syncType = ParameterSyncType.Float });

            if (parameters.AllowSaturationControl)
            {
                AddLayer(fx, "Saturation", light.Default, light.Control);

                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Saturation, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Saturation, saved = parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
            }
        }

        private static void AddLayer(AnimatorController fx, string name, AnimationClip @default, AnimationClip control)
        {
            var layer = new AnimatorControllerLayer() { name = name, defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
            var stateMachine = layer.stateMachine;
            var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = @default }.HideInHierarchy().AddTo(fx);
            var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = control, timeParameterActive = true, timeParameter = ParameterName_Value }.HideInHierarchy().AddTo(fx);

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

        private static void ConfigureResetParamerters(LightLimitChangerSettings settings)
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
            if (settings.Parameters.AllowSaturationControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Saturation, value = 0.5f });

            stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

            fx.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);
            settings.gameObject.GetOrAddComponent<ModularAvatarParameters>().parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Reset, syncType = ParameterSyncType.Bool, localOnly = true, saved = false });

            fx.AddLayer(layer);
        }

        private static VRCExpressionsMenu CreateMenu(AnimatorController fx, LightLimitChangeParameters parameters)
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
    }
}
