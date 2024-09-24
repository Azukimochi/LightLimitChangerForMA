using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

namespace gomoru.su
{
    internal sealed class MotionTree : IDirectBlendTreeItem
    {
        public MotionTree(Motion motion)
        {
            Motion = motion;
        }

        public Motion Motion { get; set; }

        public IEnumerable<AnimationClip> GetAnimationClips()
        {
            if (Motion is AnimationClip clip)
                yield return clip;
            else if (Motion is BlendTree blendTree)
                foreach (var x in Recurse(blendTree))
                    yield return x;

            IEnumerable<AnimationClip> Recurse(BlendTree blendTree)
            {
                if (blendTree == null)
                    yield break;

                foreach (var x in blendTree.children)
                {
                    var m = x.motion;
                    if (m is AnimationClip clip)
                        yield return clip;
                    else if (m is BlendTree tree)
                        foreach (var y in Recurse(tree))
                            yield return y;
                }
            }
        }

        void IDirectBlendTreeItem.Apply(BlendTree destination, Object assetContainer)
        {
            destination.AddChild(Motion);
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static MotionTree AddMotion<T>(this T directBlendTree, Motion motion) where T : IDirectBlendTreeContainer => new MotionTree(motion).AddTo(directBlendTree);
    }
}
