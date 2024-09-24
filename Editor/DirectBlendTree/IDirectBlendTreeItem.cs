using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su
{
    internal interface IDirectBlendTreeItem
    {
        void Apply(BlendTree destination, Object assetContainer);

        IEnumerable<AnimationClip> GetAnimationClips();
    }
}
