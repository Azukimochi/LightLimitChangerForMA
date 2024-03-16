namespace gomoru.su
{
    internal static partial class DirectBlendTreeExtensions
    {
        private static T AddTo<T, TContainer>(this T tree, TContainer blendTree) where T : IDirectBlendTreeItem where TContainer : IDirectBlendTreeContainer
        {
            blendTree.Add(tree);
            return tree;
        }

        private static T AddTo<T, TContainer>(this T tree, TContainer blendTree, DirectBlendTree.Target target) where T : IDirectBlendTreeItem where TContainer : IDirectBlendTreeONOFFContainer
        {
            blendTree.Add(tree, target);
            return tree;
        }
    }

    internal interface IDirectBlendTreeContainer
    {
        void Add(IDirectBlendTreeItem tree);
    }

    internal interface IDirectBlendTreeONOFFContainer
    {
        void Add(IDirectBlendTreeItem tree, DirectBlendTree.Target target);
    }
}
