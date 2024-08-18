namespace gomoru.su
{
    internal abstract class DirectBlendTreeONOFFItemBase : DirectBlendTreeItemBase, IDirectBlendTreeONOFFContainer
    {
        public IDirectBlendTreeItem ON { get; set; }
        public IDirectBlendTreeItem OFF { get; set; }

        void IDirectBlendTreeONOFFContainer.Add(IDirectBlendTreeItem tree, DirectBlendTree.Target target)
            => Add(tree, target);

        protected virtual void Add(IDirectBlendTreeItem tree, DirectBlendTree.Target target)
        {
            if (target == DirectBlendTree.Target.OFF)
                OFF = tree;
            else if (target == DirectBlendTree.Target.ON)
                ON = tree;
        }
    }
}
