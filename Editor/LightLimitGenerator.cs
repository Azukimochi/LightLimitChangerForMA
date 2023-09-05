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
        private const string AnimationKeyPrefix = "material.";

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
            var fx = settings.FX;
            var parameters = settings.Parameters;

            if (BuildManager.IsRunning)
            {
                var objectMapper = ObjectMapper.Create();
                var components = avatar.GetComponentsInChildren<Component>().Where(x => !(x is Transform)).Select(x => new SerializedObject(x)).ToArray();
                using (var baker = new TextureBaker())
                {
                    objectMapper.MapObject<Material>(components, x =>
                    {
                        if (ShaderInfo.TryGetShaderInfo(x, out var info))
                        {
                            if (parameters.TargetShader.HasFlag(info.ShaderType))
                            {
                                x = x.Clone().AddTo(fx);
                                if (parameters.AllowColorTempControl || parameters.AllowSaturationControl)
                                {
                                    info.TryNormalizeMaterial(x, baker);
                                    baker.ResetParamerter();
                                }

                                if (info.ShaderType == Shaders.Poiyomi)
                                {
                                    ShaderInfo.Poiyomi.EnableColorAdjust(x);
                                    ShaderInfo.Poiyomi.SetAnimatedFlags(x, parameters.AllowColorTempControl, parameters.AllowSaturationControl);
                                }

                                return x;
                            }
                        }
                        return null;
                    });
                }

                var animatorMapper = new AnimatorControllerMapper(objectMapper.MappedObjects, fx);

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
                                        if (ShaderInfo.TryGetShaderType(material, out var shaderType) && (shaderType & parameters.TargetShader) != 0)
                                        {
                                            goto CloningAnimator;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return null;

                CloningAnimator:
                    return animatorMapper.MapController(x);
                });
            }

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
                saturation.AddTo(fx);
            }
            if (parameters.AllowUnlitControl)
            {
                unlit.AddTo(fx);
            }

            foreach(var renderer in avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer) || settings.Parameters.ExcludeEditorOnly && renderer.tag == "EditorOnly")
                {
                    continue;
                }

                var relativePath = renderer.transform.GetRelativePath(avatar.transform);
                var type = renderer.GetType();

                var (min, max, color, color2nd, color3rd, hsvg, sat) =
                    (
                        parameters.MinLightValue,
                        parameters.MaxLightValue,
                        Color.white,
                        Color.white,
                        Color.white,
                        new Vector4(0, 1, 1, 1),
                        0f
                    );

                if (parameters.TargetShader.HasFlag(Shaders.lilToon))
                {
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._LightMinLimit}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._LightMaxLimit}", Utils.Animation.Constant(max));

                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._LightMinLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));
                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._LightMaxLimit}", Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue));

                    SetColorTemperature(false, ShaderInfo.LilToon._Color);
                    SetColorTemperature(false, ShaderInfo.LilToon._Color2nd);
                    SetColorTemperature(false, ShaderInfo.LilToon._Color3rd);

                    SetColorTemperature(true, ShaderInfo.LilToon._Color);
                    SetColorTemperature(true, ShaderInfo.LilToon._Color2nd);
                    SetColorTemperature(true, ShaderInfo.LilToon._Color3rd);

                    saturation.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._MainTexHSVG}.y", Utils.Animation.Linear(0, 2));

                    unlit.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._AsUnlit}", Utils.Animation.Constant(0));
                    unlit.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.LilToon._AsUnlit}", Utils.Animation.Linear(0.0f, 1.0f));
                }

                if (parameters.TargetShader.HasFlag(Shaders.Sunao))
                {
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._MinimumLight}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._DirectionalLight}", Utils.Animation.Constant(max));
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._PointLight}", Utils.Animation.Constant(max));
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._SHLight}", Utils.Animation.Constant(max));

                    var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._MinimumLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._DirectionalLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._PointLight}", curve);
                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._SHLight}", curve);

                    unlit.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._Unlit}", Utils.Animation.Constant(0));
                    unlit.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Sunao._Unlit}", Utils.Animation.Linear(0.0f, 1.0f));

                    SetColorTemperature(false, ShaderInfo.Sunao._Color);
                    SetColorTemperature(true, ShaderInfo.Sunao._Color);
                }

                if (parameters.TargetShader.HasFlag(Shaders.Poiyomi))
                {
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._LightingMinLightBrightness}", Utils.Animation.Constant(min));
                    light.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._LightingCap}", Utils.Animation.Constant(max));

                    var curve = Utils.Animation.Linear(parameters.MinLightValue, parameters.MaxLightValue);

                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._LightingMinLightBrightness}", curve);
                    light.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._LightingCap}", curve);

                    saturation.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._Saturation}", Utils.Animation.Constant(sat));
                    saturation.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{ShaderInfo.Poiyomi._Saturation}", Utils.Animation.Linear(-1, 1));

                    SetColorTemperature(false, ShaderInfo.Poiyomi._Color);
                    SetColorTemperature(true, ShaderInfo.Poiyomi._Color);
                }

                void SetColorTemperature(bool isControl, string parameterName)
                {
                    if (!isControl)
                    {
                        colorTemp.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.r", Utils.Animation.Constant(color.r));
                        colorTemp.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.g", Utils.Animation.Constant(color.g));
                        colorTemp.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.b", Utils.Animation.Constant(color.b));
                        colorTemp.Default.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.a", Utils.Animation.Constant(color.a));
                    }
                    else
                    {
                        colorTemp.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.r", Utils.Animation.Linear(color.r * 0.6f, color.r, color.r));
                        colorTemp.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.g", Utils.Animation.Linear(color.g * 0.95f, color.g, color.g * 0.8f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.b", Utils.Animation.Linear(color.b, color.b, color.b * 0.6f));
                        colorTemp.Control.SetCurve(relativePath, type, $"{AnimationKeyPrefix}{parameterName}.a", Utils.Animation.Constant(color.a));
                    }
                }
            }

            {
                AddLayer(fx, "Light", light.Default, light.Control, ParameterName_Value);

                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
                fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Value, defaultFloat = parameters.DefaultLightValue, type = AnimatorControllerParameterType.Float });

                var param = settings.gameObject.GetOrAddComponent<ModularAvatarParameters>();

                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = parameters.IsValueSave, defaultValue = parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Value, saved = parameters.IsValueSave, defaultValue = parameters.DefaultLightValue, syncType = ParameterSyncType.Float });

                if (parameters.AllowSaturationControl)
                {
                    AddLayer(fx, "Saturation", saturation.Default, saturation.Control, ParameterName_Saturation);

                    fx.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Saturation, defaultFloat = 0.5f, type = AnimatorControllerParameterType.Float });
                    param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Saturation, saved = parameters.IsValueSave, defaultValue = 0.5f, syncType = ParameterSyncType.Float });
                }

                if (parameters.AllowUnlitControl)
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

            var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(fx);
            {
                rootMenu.name = "Root Menu";
                rootMenu.controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control
                    {
                        name = "Light Limit Changer",
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = mainMenu,
                    },
                };
            };

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

        internal readonly struct ObjectMapper
        {
            public readonly Dictionary<Object, Object> MappedObjects;

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
                    serializedObject.ApplyModifiedProperties();
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
