using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

namespace gomoru.su
{
    internal sealed class RadialPuppetBlendTree : DirectBlendTreeItemBase
    {
        public string ParameterName { get; set; }

        public AnimationClip Animation { get; set; }

        protected override void Apply(BlendTree destination, Object assetContainer)
        {
            var blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, assetContainer);
            blendTree.blendParameter = ParameterName;
            blendTree.name = Name;
            var dict = new Dictionary<float, AnimationClip>();
            SeparateAnimationClips(Animation, assetContainer, dict);
            var max = dict.Keys.Max();
            foreach(var item in dict.OrderBy(x => x.Key))
            {
                blendTree.AddChild(item.Value, item.Key / max);
            }
            destination.AddChild(blendTree);
        }

        private static readonly ObjectReferenceKeyframe[] _singleKeyFrame = new ObjectReferenceKeyframe[1];

        private static void SeparateAnimationClips(AnimationClip clip, Object assetContainer, Dictionary<float, AnimationClip> destination)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
            {
                // Editor Curve
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve != null)
                {
                    foreach (var key in curve.keys)
                    {
                        var time = key.time;
                        var motion = GetOrAddSeparetedClip(time);
                        var singleCurve = AnimationCurve.Constant(time, time, key.value);
                        AnimationUtility.SetEditorCurve(motion, binding, singleCurve);
                    }
                }

                // Object Reference
                var objectReferences = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (objectReferences != null)
                {
                    foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                    {
                        var time = key.time;
                        var motion = GetOrAddSeparetedClip(time);

                        var copiedKey = key;
                        copiedKey.time = 0;
                        _singleKeyFrame[0] = copiedKey;
                        AnimationUtility.SetObjectReferenceCurve(motion, binding, _singleKeyFrame);
                    }
                }
            }

            AnimationClip GetOrAddSeparetedClip(float time)
            {
                if (!destination.TryGetValue(time, out var motion))
                {
                    motion = new AnimationClip() { name = $"{clip.name}.{Mathf.FloorToInt(time * clip.frameRate)}f" };
                    AssetDatabase.AddObjectToAsset(motion, assetContainer);
                    destination.Add(time, motion);
                }
                return motion;
            }
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static RadialPuppetBlendTree AddRadialPuppet<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new RadialPuppetBlendTree() { Name = name }.AddTo(directBlendTree);

        public static RadialPuppetBlendTree AddRadialPuppet<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new RadialPuppetBlendTree() { Name = name }.AddTo(directBlendTree, target);
    }
}
