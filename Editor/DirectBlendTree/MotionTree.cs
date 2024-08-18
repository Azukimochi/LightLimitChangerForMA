using UnityEngine;
using UnityEditor.Animations;

namespace gomoru.su
{
    internal sealed class MotionTree : IDirectBlendTreeItem
    {
        public MotionTree(Motion motion)
        {
            Motion = motion;
        }

        public Motion Motion { get; set; }

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
