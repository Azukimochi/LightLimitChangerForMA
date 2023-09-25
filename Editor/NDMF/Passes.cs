using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using nadena.dev.ndmf.util;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        public static void RunningPasses(Sequence sequence)
        {
            _isStateInitialized = false;

            sequence
            .Run(CloningMaterials).Then
            .Run(NormalizeMaterials).Then
            .Run(GenerateAnimations).Then
            .Run(Finalize);
        }

        public readonly static CloningMaterialsPass CloningMaterials = new CloningMaterialsPass();
        public readonly static NormalizeMaterialsPass NormalizeMaterials = new NormalizeMaterialsPass();
        public readonly static GenerateAnimationsPass GenerateAnimations = new GenerateAnimationsPass();
        public readonly static FinalizePass Finalize = new FinalizePass();

        internal const string ParameterName_Toggle = "LightLimitEnable";
        internal const string ParameterName_Value = "LightLimitValue";
        internal const string ParameterName_Saturation = "LightLimitSaturation";
        internal const string ParameterName_Unlit = "LightLimitUnlit";
        internal const string ParameterName_ColorTemp = "LightLimitColorTemp";
        internal const string ParameterName_Reset = "LightLimitReset";

        private static bool _isStateInitialized = false;

        private static Session GetSession(BuildContext context)
        {
            var session = context.GetState<Session>();
            if (!_isStateInitialized)
            {
                session.Settings = context.AvatarRootObject.GetComponentInChildren<LightLimitChangerSettings>();
                session.Parameters = session.Settings?.Parameters ?? LightLimitChangerParameters.Default;
                session.Controller = new AnimatorController() { name = "Light Limit Controller" }.AddTo(context.AssetContainer);

                _isStateInitialized = true;
            }
            return session;
        }

        internal sealed class CloningMaterialsPass : Pass<CloningMaterialsPass>
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid())
                    return;
                var components = context.AvatarRootObject.GetComponentsInChildren<Component>(true);
                var mapper = new AnimatorControllerMapper(session.ObjectMapping, context.AssetContainer);

                foreach (var component in components)
                {
                    var so = new SerializedObject(component);
                    bool enterChildren = true;
                    var p = so.GetIterator();
                    while (p.Next(enterChildren))
                    {
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            var obj = p.objectReferenceValue;
                            if (obj != null)
                            {
                                if (session.ObjectMapping.TryGetValue(obj, out var mapped))
                                {
                                    p.objectReferenceValue = mapped;
                                }
                                else if (obj is Material mat)
                                {
                                    if (TryClone(mat, out var cloned))
                                    {
                                        p.objectReferenceValue = cloned;
                                    }
                                }
                                else if (obj is RuntimeAnimatorController runtimeAnimatorController)
                                {
                                    bool needClone = false;
                                    foreach (var material in runtimeAnimatorController.GetAnimatedMaterials())
                                    {
                                        if (TryClone(material, out var cloned))
                                        {
                                            needClone = true;
                                        }
                                    }
                                    if (needClone)
                                    {
                                        var c = mapper.MapController(runtimeAnimatorController);
                                        p.objectReferenceValue = c;
                                    }
                                }
                            }

                            bool TryClone(Material material, out Material clonedMaterial)
                            {
                                if (!session.ObjectMapping.TryGetValue(material, out var mapped))
                                {
                                    if (ShaderInfo.TryGetShaderInfo(material, out var info) && session.Parameters.TargetShader.HasFlag(info.ShaderType))
                                    {
                                        clonedMaterial = material.Clone().AddTo(context.AssetContainer);
                                        session.ObjectMapping.Add(material, clonedMaterial);
                                        return true;
                                    }
                                    clonedMaterial = null;
                                    return false;
                                }
                                else
                                {
                                    clonedMaterial = mapped as Material;
                                    return true;
                                }
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
                            default:
                                enterChildren = true;
                                break;
                        }
                    }

                    so.ApplyModifiedProperties();
                }
            }
        }

        internal sealed class NormalizeMaterialsPass : Pass<NormalizeMaterialsPass>
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid() || (!session.Parameters.AllowColorTempControl && !session.Parameters.AllowSaturationControl))
                    return;

                foreach (var material in session.ObjectMapping.Values.Select(x => x as Material).Where(x => x != null))
                {
                    if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo))
                    {
                        shaderInfo.TryNormalizeMaterial(material, context.AssetContainer);
                    }
                }
            }
        }

        internal sealed class GenerateAnimationsPass : Pass<GenerateAnimationsPass>
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid())
                    return;

                var controller = session.Controller;

                ReadOnlySpan<ControlAnimationContainer> animationContainers = new[]
                {
                    ControlAnimationContainer.Create(LightLimitControlType.Light, "Light"),
                    ControlAnimationContainer.Create(LightLimitControlType.Saturation, "Saturation"),
                    ControlAnimationContainer.Create(LightLimitControlType.Unlit, "Unlit"),
                    ControlAnimationContainer.Create(LightLimitControlType.ColorTemperature, "ColorTemp"),
                };

                var parameters = session.Parameters;
                var targetControl = LightLimitControlType.Light;


                if (parameters.AllowColorTempControl)
                {
                    targetControl |= LightLimitControlType.ColorTemperature;
                }
                if (parameters.AllowSaturationControl)
                {
                    targetControl |= LightLimitControlType.Saturation;
                }
                if (parameters.AllowUnlitControl)
                {
                    targetControl |= LightLimitControlType.Unlit;
                }

                foreach (var renderer in context.AvatarRootObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer) || (parameters.ExcludeEditorOnly && renderer.CompareTag("EditorOnly")))
                    {
                        continue;
                    }

                    var relativePath = renderer.AvatarRootPath();
                    var type = renderer.GetType();

                    var controlParameters = new ControlAnimationParameters(relativePath, type, parameters.MinLightValue, parameters.MaxLightValue);

                    foreach (var x in ShaderInfo.RegisteredShaderInfos)
                    {
                        if (!parameters.TargetShader.HasFlag(x.ShaderType))
                            continue;

                        foreach (ref readonly var container in animationContainers)
                        {
                            x.SetControlAnimation(container, controlParameters);
                        }
                    }
                }

                var param = session.Settings.gameObject.GetOrAddComponent<ModularAvatarParameters>();
                controller.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
                param.parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Toggle, saved = parameters.IsValueSave, defaultValue = parameters.IsDefaultUse ? 1 : 0, syncType = ParameterSyncType.Bool });

                foreach (ref readonly var container in animationContainers)
                {
                    if (targetControl.HasFlag(container.ControlType))
                    {
                        var (defaultValue, parameterName) =
                            container.ControlType == LightLimitControlType.Light ? (parameters.DefaultLightValue, ParameterName_Value) :
                            container.ControlType == LightLimitControlType.Saturation ? (0.5f, ParameterName_Saturation) :
                            container.ControlType == LightLimitControlType.Unlit ? (0.0f, ParameterName_Unlit) :
                            container.ControlType == LightLimitControlType.ColorTemperature ? (0.5f, ParameterName_ColorTemp) :
                            (0f, null);

                        if (parameterName is null)
                            continue;

                        container.AddTo(context.AssetContainer);
                        AddLayer(controller, container, parameterName);

                        controller.AddParameter(new AnimatorControllerParameter() { name = parameterName, defaultFloat = defaultValue, type = AnimatorControllerParameterType.Float });
                        param.parameters.Add(new ParameterConfig() { nameOrPrefix = parameterName, saved = parameters.IsValueSave, defaultValue = defaultValue, syncType = ParameterSyncType.Float });
                    }
                }

                if (!session.Parameters.AddResetButton)
                    return;

                AnimatorStateMachine stateMachine;
                var layer = new AnimatorControllerLayer()
                {
                    name = "Reset",
                    stateMachine = stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(context.AssetContainer),
                    defaultWeight = 1,
                };
                var blank = new AnimationClip() { name = "Blank" }.HideInHierarchy().AddTo(context.AssetContainer);
                var off = new AnimatorState() { name = "Off", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(context.AssetContainer);
                var on = new AnimatorState() { name = "On", writeDefaultValues = false, motion = blank }.HideInHierarchy().AddTo(context.AssetContainer);

                var cond = new AnimatorCondition[] { new AnimatorCondition() { mode = AnimatorConditionMode.If, parameter = ParameterName_Reset } };

                var t = new AnimatorStateTransition()
                {
                    destinationState = on,
                    duration = 0,
                    hasExitTime = false,
                    conditions = cond
                }.HideInHierarchy().AddTo(context.AssetContainer);

                off.AddTransition(t);

                cond[0].mode = AnimatorConditionMode.IfNot;
                t = new AnimatorStateTransition()
                {
                    destinationState = off,
                    duration = 0,
                    hasExitTime = false,
                    conditions = cond
                }.HideInHierarchy().AddTo(context.AssetContainer);

                on.AddTransition(t);

                var dr = on.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Value, value = session.Parameters.DefaultLightValue });
                if (session.Parameters.AllowColorTempControl)
                    dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_ColorTemp, value = 0.5f });
                if (session.Parameters.AllowSaturationControl)
                    dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Saturation, value = 0.5f });
                if (session.Parameters.AllowUnlitControl)
                    dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Unlit, value = 0.0f });
                stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
                stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

                session.Controller.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);

                session.Controller.AddLayer(layer);
            }

            private static void AddLayer(AnimatorController fx, ControlAnimationContainer container, string parameterName)
            {
                var layer = new AnimatorControllerLayer() { name = container.Name, defaultWeight = 1, stateMachine = new AnimatorStateMachine().HideInHierarchy().AddTo(fx) };
                var stateMachine = layer.stateMachine;
                var defaultState = new AnimatorState() { name = "Default", writeDefaultValues = false, motion = container.Default }.HideInHierarchy().AddTo(fx);
                var state = new AnimatorState() { name = "Control", writeDefaultValues = false, motion = container.Control, timeParameterActive = true, timeParameter = parameterName }.HideInHierarchy().AddTo(fx);

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
        }

        internal sealed class FinalizePass : Pass<FinalizePass>
        {
            protected override void Execute(BuildContext context)
            {
                var session = GetSession(context);
                if (!session.IsValid())
                    return;

                var obj = session.Settings.gameObject;
                var mergeAnimator = obj.GetOrAddComponent<ModularAvatarMergeAnimator>();
                var maParameters = obj.GetOrAddComponent<ModularAvatarParameters>();
                var menuInstaller = obj.GetOrAddComponent<ModularAvatarMenuInstaller>();

                mergeAnimator.animator = session.Controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;

                maParameters.parameters.AddRange(session.Controller.parameters.Select(x => new ParameterConfig()
                {
                    nameOrPrefix = x.name,
                    internalParameter = true,
                    syncType = x.type == AnimatorControllerParameterType.Float ? ParameterSyncType.Float : ParameterSyncType.Bool,
                    defaultValue = x.type == AnimatorControllerParameterType.Float ? x.defaultFloat : (x.defaultBool ? 1 : 0),
                    saved = session.Parameters.IsValueSave,
                    localOnly = x.name == ParameterName_Reset
                }));

                menuInstaller.menuToAppend = CreateMenu(context);
            }


            private static VRCExpressionsMenu CreateMenu(BuildContext context)
            {
                var session = GetSession(context);
                var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(context.AssetContainer);
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

                if (session.Parameters.AllowColorTempControl)
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

                if (session.Parameters.AllowSaturationControl)
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

                if (session.Parameters.AllowUnlitControl)
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

                if (session.Parameters.AddResetButton)
                {
                    mainMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Reset",
                        type = VRCExpressionsMenu.Control.ControlType.Button,
                        parameter = new VRCExpressionsMenu.Control.Parameter() { name = ParameterName_Reset }
                    });
                }

                var rootMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>().AddTo(context.AssetContainer);
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
        }

        private sealed class Session
        {
            public LightLimitChangerSettings Settings;
            public LightLimitChangerParameters Parameters;
            public Dictionary<Object, Object> ObjectMapping = new Dictionary<Object, Object>();
            public AnimatorController Controller;

            public bool IsValid() => Settings != null;
        }

    }

}
