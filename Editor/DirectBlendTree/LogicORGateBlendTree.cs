using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace gomoru.su
{
    internal sealed class LogicORGateBlendTree : DirectBlendTreeONOFFItemBase
    {
        public string[] Parameters { get; set; }

        protected override void Apply(BlendTree destination, Object assetContainer)
        {
            var blendTree = new BlendTree();
            OFF.Apply(blendTree, assetContainer);
            ON.Apply(blendTree, assetContainer);
            blendTree.blendParameter = Parameters[Parameters.Length - 1];
            AssetDatabase.AddObjectToAsset(blendTree, assetContainer);

            for (int i = Parameters.Length - 2; i >= 0; i--)
            {
                var tree = new BlendTree();
                AssetDatabase.AddObjectToAsset(tree, assetContainer);
                tree.AddChild(blendTree);
                ON.Apply(tree, assetContainer);
                tree.blendParameter = Parameters[i];

                blendTree = tree;
            }

            blendTree.name = Name;
            destination.AddChild(blendTree);
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static LogicORGateBlendTree AddOrGate<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new LogicORGateBlendTree() { Name = name }.AddTo(directBlendTree);

        public static LogicORGateBlendTree AddOrGate<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new LogicORGateBlendTree() { Name = name }.AddTo(directBlendTree, target);
    }
}
