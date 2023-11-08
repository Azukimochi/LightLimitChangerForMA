using System;
using System.Collections.Generic;
using Anatawa12.AvatarOptimizer;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static io.github.azukimochi.Passes;
using Object = UnityEngine.Object;

namespace io.github.azukimochi
{
    partial class Passes
    {
        internal sealed class CloningMaterialsPass : LightLimitChangerBasePass<CloningMaterialsPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var components = context.AvatarRootObject.GetComponentsInChildren<Component>(true);
                var mapper = new AnimatorControllerMapper(cache);
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
                            if (obj != null && !session.Excludes.Contains(obj))
                            {
                                if (cache.TryGetValue(obj, out var mapped))
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
                                    if (cache.TryGetValue(runtimeAnimatorController, out var mappedController))
                                    {
                                        p.objectReferenceValue = mappedController;
                                    }
                                    else
                                    {
                                        bool needClone = false;
                                        foreach (var material in runtimeAnimatorController.GetAnimatedMaterials())
                                        {
                                            needClone |= TryClone(material, out _);
                                        }
                                        if (needClone)
                                        {
                                            p.objectReferenceValue = mapper.MapAnimatorController(runtimeAnimatorController);
                                        }
                                    }
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
                        so.ApplyModifiedProperties();
                    }
                }
            }

            internal bool TryClone(Material material, out Material clonedMaterial)
            {
                if (!Cache.TryGetValue(material, out var mapped))
                {
                    if (ShaderInfo.TryGetShaderInfo(material, out var info) && Session.Parameters.TargetShaders.Contains(info.Name))
                    {
                        clonedMaterial = material.Clone();
                        Cache.Register(material, clonedMaterial);
                        return true;
                    }
                    clonedMaterial = null;
                    return false;
                }
                else
                {
                    clonedMaterial = mapped;
                    return true;
                }
            }
        }
    }
}
