using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

namespace gomoru.su
{
    internal sealed partial class DirectBlendTree : IDirectBlendTreeItem, IDirectBlendTreeContainer
    {
        private List<IDirectBlendTreeItem> _items;

        public string Name { get; set; }

        public string ParameterName { get; }

        public static string DirectBlendParameter { get; set; } = "1";

        public DirectBlendTree(string parameterName = null)
        {
            _items = new List<IDirectBlendTreeItem>();
            ParameterName = parameterName ?? DirectBlendParameter;
        }

        public void Add(IDirectBlendTreeItem item) => _items.Add(item);

        public BlendTree ToBlendTree(Object assetContainer)
        {
            var blendTree = new BlendTree();
            blendTree.name = Name;
            AssetDatabase.AddObjectToAsset(blendTree, assetContainer);
            blendTree.blendType = BlendTreeType.Direct;
            SetNormalizedBlendValues(blendTree, false);
            foreach (var item in _items)
            {
                item.Apply(blendTree, assetContainer);
            }

            var children = blendTree.children;
            for(int i = 0; i < children.Length; i++)
            {
                children[i].directBlendParameter = ParameterName;
            }
            blendTree.children = children;

            return blendTree;
        }

        public AnimatorControllerLayer ToAnimatorControllerLayer(Object assetContainer)
        {
            var layer = new AnimatorControllerLayer();
            layer.name = Name;
            var stateMachine = layer.stateMachine = new AnimatorStateMachine();
            AssetDatabase.AddObjectToAsset(stateMachine, assetContainer);

            var state = stateMachine.AddState($"{(Name ?? "Direct Blend Tree")} (WD ON)");
            state.writeDefaultValues = true;
            state.motion = ToBlendTree(assetContainer);

            return layer;
        }

        void IDirectBlendTreeItem.Apply(BlendTree destination, Object assetContainer)
        {
            var blendTree = ToBlendTree(assetContainer);
            destination.AddChild(blendTree);
        }

        private static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
        {
            using (var so = new SerializedObject(blendTree))
            {
                so.FindProperty("m_NormalizedBlendValues").boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public enum Target
        {
            OFF,
            ON
        }
    }

    static partial class DirectBlendTreeExtensions
    {
        public static DirectBlendTree AddDirectBlendTree<T>(this T directBlendTree, string name = null) where T : IDirectBlendTreeContainer => new DirectBlendTree(directBlendTree is DirectBlendTree tree ? tree.ParameterName : "1") { Name = name }.AddTo(directBlendTree);

        public static DirectBlendTree AddDirectBlendTree<T>(this T directBlendTree, DirectBlendTree.Target target, string name = null) where T : IDirectBlendTreeONOFFContainer => new DirectBlendTree(directBlendTree is DirectBlendTree tree ? tree.ParameterName : null) { Name = name }.AddTo(directBlendTree, target);
    }
}
