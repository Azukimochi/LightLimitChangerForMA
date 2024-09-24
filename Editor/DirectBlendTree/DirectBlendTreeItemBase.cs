using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

namespace gomoru.su
{
    internal abstract class DirectBlendTreeItemBase : IDirectBlendTreeItem
    {
        public string Name { get; set; }

        void IDirectBlendTreeItem.Apply(BlendTree destination, Object assetContainer) => Apply(destination, assetContainer);

        public abstract IEnumerable<AnimationClip> GetAnimationClips();

        protected abstract void Apply(BlendTree destination, Object assetContainer);


        protected static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
        {
            using (var so = new SerializedObject(blendTree))
            {
                so.FindProperty("m_NormalizedBlendValues").boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
