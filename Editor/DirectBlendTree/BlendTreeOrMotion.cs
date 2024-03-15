using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su
{
    internal sealed class BlendTreeOrMotion : IDirectBlendTreeItem
    {
        public IDirectBlendTreeItem BlendTree { get; set; }
        public Motion Motion { get; set; }

        public BlendTreeOrMotion(IDirectBlendTreeItem blendTree) : this(blendTree, null) { }
        public BlendTreeOrMotion(Motion motion) : this(null, motion) { }
        
        public BlendTreeOrMotion(IDirectBlendTreeItem blendTree, Motion motion)
        {
            BlendTree = blendTree;
            Motion = motion;
        }

        public void Apply(BlendTree destination, Object assetContainer)
        {
            if (BlendTree != null)
            {
                BlendTree.Apply(destination, assetContainer);
            }
            else if (Motion != null)
            {
                destination.AddChild(Motion);
            }
        }

        public static implicit operator BlendTreeOrMotion(Motion motion) => new BlendTreeOrMotion(motion);

    }
}
