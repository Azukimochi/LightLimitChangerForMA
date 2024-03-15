using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace gomoru.su
{
    internal sealed class LogicANDGateBlendTree : DirectBlendTreeONOFFItemBase
    {
        public string[] Parameters { get; set; }

        protected override void Apply(BlendTree destination, Object assetContainer)
        {
            BlendTree root = new BlendTree();
            var blendTree = root;
            for(int i = 0; i < Parameters.Length; i++)
            {
                AssetDatabase.AddObjectToAsset(blendTree, assetContainer);
                blendTree.name = Parameters[i];
                blendTree.blendParameter = Parameters[i];
                OFF.Apply(blendTree, assetContainer);
                if (i != Parameters.Length - 1)
                {
                    var blendTree2 = new BlendTree();
                    blendTree.AddChild(blendTree2);
                    blendTree = blendTree2;
                }
                else
                {
                    ON.Apply(blendTree, assetContainer);
                }
            }
            
            destination.AddChild(root);
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static LogicANDGateBlendTree AddAndGate<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new LogicANDGateBlendTree() { Name = name }.AddTo(directBlendTree);

        public static LogicANDGateBlendTree AddAndGate<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new LogicANDGateBlendTree() { Name = name }.AddTo(directBlendTree, target);
    }
}
