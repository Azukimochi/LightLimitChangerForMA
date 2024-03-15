using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace gomoru.su
{
    internal sealed class ToggleBlendTree : DirectBlendTreeONOFFItemBase
    {
        public string ParameterName { get; set; }

        protected override void Apply(BlendTree destination, Object assetContainer)
        {
            var blendTree = new BlendTree();
            AssetDatabase.AddObjectToAsset(blendTree, assetContainer);
            blendTree.blendParameter = ParameterName;
            blendTree.name = Name;
            OFF.Apply(destination, assetContainer);
            ON.Apply(destination, assetContainer);

            destination.AddChild(blendTree);
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static ToggleBlendTree AddToggle<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new ToggleBlendTree() { Name = name }.AddTo(directBlendTree);

        public static ToggleBlendTree AddToggle<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new ToggleBlendTree() { Name = name }.AddTo(directBlendTree, target);
    }
}
