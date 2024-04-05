using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace io.github.azukimochi
{
    internal static partial class Passes
    {
        internal sealed class CollectTargetRenderersPass : LightLimitChangerBasePass<CollectTargetRenderersPass>
        {
            protected override void Execute(BuildContext context, Session session, LightLimitChangerObjectCache cache)
            {
                var list = session.TargetRenderers;

                CollectMeshRenderers(context.AvatarRootObject, session.Parameters.TargetShaders, list);
                CollectMeshRenderersInAnimation(context.AvatarRootObject, session.Parameters.TargetShaders, list);

                if (list.Count == 0)
                {
                    IError error = new ErrorMessage("NDMF.info.non_generated", ErrorSeverity.NonFatal);
                    ErrorReport.ReportError(error);
                    session.IsFailed = true;
                }
            }

            private static void CollectMeshRenderers(GameObject avatarObject, in TargetShaders targetShaders, HashSet<Renderer> list)
            {
                foreach (var renderer in avatarObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.CompareTag("EditorOnly") || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                    {
                        continue;
                    }

                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo) && targetShaders.Contains(shaderInfo.Name))
                        {
                            list.Add(renderer);
                            break;
                        }
                    }
                }
            }

            private static void CollectMeshRenderersInAnimation(GameObject avatarObject, in TargetShaders targetShaders, HashSet<Renderer> list)
            {
                var components = avatarObject.GetComponentsInChildren<Component>(true);
                var clips = new Dictionary<AnimationClip, Component>();
                foreach (var component in components)
                {
                    var so = new SerializedObject(component);
                    bool enterChildren = true;
                    var p = so.GetIterator();
                    while (p.Next(enterChildren))
                    {
                        if (p.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (p.objectReferenceValue is RuntimeAnimatorController controller)
                            {
                                foreach (var x in controller.animationClips)
                                {
                                    if (!clips.ContainsKey(x))
                                        clips.Add(x, component);
                                }
                            }
                        }

                        enterChildren = p.propertyType.IsNeedToEnterChildren();
                    }
                }

                foreach (var (clip, component) in clips)
                {
                    foreach (var bind in AnimationUtility.GetObjectReferenceCurveBindings(clip) ?? Array.Empty<EditorCurveBinding>())
                    {
                        var rootObj = component.gameObject;
                        if (component is ModularAvatarMergeAnimator mamaaaa && mamaaaa.pathMode == MergeAnimatorPathMode.Absolute)
                        {
                            rootObj = avatarObject;
                        }

                        var obj = AnimationUtility.GetAnimatedObject(rootObj, bind);
                        if (obj is MeshRenderer || obj is SkinnedMeshRenderer)
                        {
                            var renderer = obj as Renderer;
                            foreach (var material in AnimationUtility.GetObjectReferenceCurve(clip, bind).Select(x => x.value as Material).Where(x => x != null))
                            {
                                if (ShaderInfo.TryGetShaderInfo(material, out var shaderInfo) && targetShaders.Contains(shaderInfo.Name))
                                {
                                    list.Add(renderer);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
