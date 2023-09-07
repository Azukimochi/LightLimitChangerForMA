using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
        private const string ParameterName_Toggle = "LightLimitEnable";
        private const string ParameterName_Value = "LightLimitValue";
        private const string ParameterName_Saturation = "LightLimitSaturation";
        private const string ParameterName_Unlit = "LightLimitUnlit";
        private const string ParameterName_ColorTemp = "LightLimitColorTemp";
        private const string ParameterName_Reset = "LightLimitReset";

        private struct BuildContext
        {
            public VRCAvatarDescriptor Avatar;
            public GameObject Object;
            public AnimatorController Controller;
            public Object AssetContainer;
            public LightLimitChangerParameters Parameters;
        }

        public static void Generate(VRCAvatarDescriptor avatar, LightLimitChangerSettings settings)
        {
            var container = settings.AssetContainer;
            if (container == null)
                settings.AssetContainer = container = CreateTemporaryAsset();

            var fx = new AnimatorController().AddTo(container);

            var obj = settings.gameObject;
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

            container.ClearSubAssets();

            var context = new BuildContext()
            {
                Avatar = avatar,
                Controller = fx,
                Object = obj,
                AssetContainer = container,
                Parameters = settings.Parameters,
            };

            ConfigureControls(context);
            ConfigureResetParameters(context);

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureControls(in BuildContext context)
        {
            var assetContainer = context.AssetContainer;
            var fx = context.Controller;
            var parameters = context.Parameters;

            if (BuildManager.IsRunning)
            {
                var objectMapper = new ObjectMapper(assetContainer);
                var components = context.Avatar.GetComponentsInChildren<Component>().Where(x => !(x is Transform)).Select(x => new SerializedObject(x)).ToArray();

                using (var baker = new TextureBaker(assetContainer))
                {
                    objectMapper.MapObject(components, obj =>
                    {
                        if (obj is Material mat)
                        {
                            return CloneAndNormalizeMaterial(mat);
                        }
                        else if (obj is RuntimeAnimatorController runtimeAnimatorController)
                        {
                            bool needAnimatorMapping = false;
                            foreach (var material in runtimeAnimatorController.GetAnimatedMaterials())
                            {
                                if (!objectMapper.MappedObjects.ContainsKey(material))
                                {
                                    var result = CloneAndNormalizeMaterial(material);
                                    if (result != null)
                                    {
                                        objectMapper.MappedObjects.Add(material, result);
                                        needAnimatorMapping = true;
                                    }
                                }
                                else
                                {
                                    needAnimatorMapping = true;
                                }
                            }

                            if (needAnimatorMapping)
                            {
                                return objectMapper.MapController(runtimeAnimatorController);
                            }
                        }

                        return null;
                    });

                    Material CloneAndNormalizeMaterial(Material material)
                    {
                        if (ShaderInfo.TryGetShaderInfo(material, out var info) && parameters.TargetShader.HasFlag(info.ShaderType))
                        {
                            material = material.Clone().AddTo(assetContainer);
                            if (parameters.AllowColorTempControl || parameters.AllowSaturationControl)
                            {
                                baker.ResetParamerter();
                                info.TryNormalizeMaterial(material, baker);
                            }

                            info.AdditionalControl(material, parameters);

                            return material;
                        }

                        return null;
                    }
                }
            }

            ReadOnlySpan<ControlAnimationContainer> animationContainers = new[]
            {
                ControlAnimationContainer.Create(LightLimitControlType.Light, "Light"),
                ControlAnimationContainer.Create(LightLimitControlType.Saturation, "Saturation"),
                ControlAnimationContainer.Create(LightLimitControlType.Unlit, "Unlit"),
                ControlAnimationContainer.Create(LightLimitControlType.ColorTemperature, "ColorTemp"),
            };

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

            foreach(var renderer in context.Avatar.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer) || (parameters.ExcludeEditorOnly && renderer.CompareTag("EditorOnly")))
                {
                    continue;
                }

                var relativePath = renderer.transform.GetRelativePath(context.Avatar.transform);
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

            var param = context.Object.GetOrAddComponent<ModularAvatarParameters>();
            context.Controller.AddParameter(new AnimatorControllerParameter() { name = ParameterName_Toggle, defaultBool = parameters.IsDefaultUse, type = AnimatorControllerParameterType.Bool });
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

                    container.AddTo(assetContainer);
                    AddLayer(fx, container, parameterName);

                    fx.AddParameter(new AnimatorControllerParameter() { name = parameterName, defaultFloat = defaultValue, type = AnimatorControllerParameterType.Float });
                    param.parameters.Add(new ParameterConfig() { nameOrPrefix = parameterName, saved = parameters.IsValueSave, defaultValue = defaultValue, syncType = ParameterSyncType.Float });
                }
            }
        }

        private static void ConfigureResetParameters(in BuildContext context)
        {
            if (!context.Parameters.AddResetButton)
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
            dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Value, value = context.Parameters.DefaultLightValue });
            if (context.Parameters.AllowColorTempControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_ColorTemp, value = 0.5f });
            if (context.Parameters.AllowSaturationControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Saturation, value = 0.5f });
            if (context.Parameters.AllowUnlitControl)
                dr.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter() { type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set, name = ParameterName_Unlit, value = 0.0f });
            stateMachine.AddState(off, stateMachine.entryPosition + new Vector3(-20, 50));
            stateMachine.AddState(on, stateMachine.entryPosition + new Vector3(-20, 100));

            context.Controller.AddParameter(ParameterName_Reset, AnimatorControllerParameterType.Bool);
            context.Object.GetOrAddComponent<ModularAvatarParameters>().parameters.Add(new ParameterConfig() { nameOrPrefix = ParameterName_Reset, syncType = ParameterSyncType.Bool, localOnly = true, saved = false });

            context.Controller.AddLayer(layer);
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

        private static AssetContainer CreateTemporaryAsset()
        {
            var container = ScriptableObject.CreateInstance<AssetContainer>(); 
            AssetDatabase.CreateAsset(container, System.IO.Path.Combine(Utils.GetGeneratedAssetsFolder(), $"{GUID.Generate()}.asset"));
            AssetDatabase.SaveAssets();
            return container;
        }

        // Original: https://github.com/anatawa12/AvatarOptimizer/blob/f31d71e318a857b4f4d7db600f043c3ba5d26918/Editor/Processors/ApplyObjectMapping.cs#L136-L303
        // Originally under MIT License
        // Copyright (c) 2022 anatawa12
        internal sealed class ObjectMapper
        {
            public readonly Dictionary<Object, Object> MappedObjects = new Dictionary<Object, Object>();
            public readonly Dictionary<Object, Object> _cache = new Dictionary<Object, Object>();
            private readonly Object _rootArtifact;
            private bool _mapped = false;

            public ObjectMapper(Object rootArtifact)
            {
                _rootArtifact = rootArtifact;
            }

            public void MapObject(SerializedObject[] objects, Func<Object, Object> factory)
            {
                foreach (var serializedObject in objects)
                {
                    bool enterChildren = true;
                    var p = serializedObject.GetIterator();
                    while (p.Next(enterChildren))
                    {
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            var obj = p.objectReferenceValue;
                            if (obj != null)
                            {
                                if (!MappedObjects.TryGetValue(obj, out var mapped))
                                {
                                    mapped = factory(obj);
                                    if (mapped != null)
                                    {
                                        MappedObjects.Add(obj, mapped);
                                        p.objectReferenceValue = mapped;
                                    }
                                }
                                else
                                {
                                    p.objectReferenceValue = mapped;
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
                            if (curves[i2].value is Material material && MappedObjects.TryGetValue(material, out var mapped))
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
                            if (x.value is Material material && MappedObjects.TryGetValue(material, out var mapped))
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
