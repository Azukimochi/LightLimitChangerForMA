using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su
{
    internal sealed class MotionBlendTree : DirectBlendTreeItemBase
    {
        public string ParameterName { get; set; }

        public List<Motion> Motions { get; } = new List<Motion>();

        protected override void Apply(BlendTree destination, Object assetContainer)
        {
            var blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, assetContainer);
            blendTree.blendParameter = ParameterName;
            blendTree.name = Name;
            
            for(int i = 0; i < Motions.Count; i++)
            {
                blendTree.AddChild(Motions[i], i / (float)Motions.Count);
            }

            destination.AddChild(blendTree);
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static MotionBlendTree AddBlendTree<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new MotionBlendTree() { Name = name }.AddTo(directBlendTree);

        public static MotionBlendTree AddBlendTree<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new MotionBlendTree() { Name = name }.AddTo(directBlendTree, target);
    }
}
