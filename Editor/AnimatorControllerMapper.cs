using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    // Original: https://github.com/anatawa12/AvatarOptimizer/blob/f31d71e318a857b4f4d7db600f043c3ba5d26918/Editor/Processors/ApplyObjectMapping.cs#L136-L303
    // Originally under MIT License
    // Copyright (c) 2022 anatawa12
    internal sealed class AnimatorControllerMapper
    {
        private readonly LightLimitChangerObjectCache _cache;
        private bool _mapped = false;

        public AnimatorControllerMapper(LightLimitChangerObjectCache cache)
        {
            _cache = cache;
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
            if (_cache.TryGetValue(controller, out AnimatorController cached)) return cached;
            _mapped = false;
            var newController = new AnimatorController
            {
                parameters = controller.parameters,
                layers = controller.layers.Select(MapAnimatorControllerLayer).ToArray()
            };
            if (!_mapped) newController = null;

            return _cache.Register(controller, newController);
        }

        public AnimatorOverrideController MapAnimatorOverrideController(AnimatorOverrideController controller)
        {
            if (_cache.TryGetValue(controller, out var cached)) return cached;
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
                        if (curves[i2].value is Material material && _cache.TryGetValue(material, out var mapped))
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

                    list[i] = new KeyValuePair<AnimationClip, AnimationClip>(x.Key, _cache.Register(newClip));
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

            return _cache.Register(controller);
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
                        if (x.value is Material material && _cache.TryGetValue(material, out var mapped))
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

                _cache.Register(newClip);

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
            if (_cache.TryGetValue(original, out var cached)) return cached;


            var obj = visitor(original) as T;
            if (obj != null)
            {
                return _cache.Register(original, obj);
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

            _cache.Register(original, obj.HideInHierarchy());

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
