namespace gomoru.su
{
    internal abstract class DirectBlendTreeONOFFItemBase : DirectBlendTreeItemBase, IDirectBlendTreeONOFFContainer
    {
        public BlendTreeOrMotion ON { get; set; }
        public BlendTreeOrMotion OFF { get; set; }

        void IDirectBlendTreeONOFFContainer.Add(IDirectBlendTreeItem tree, DirectBlendTree.Target target)
            => Add(tree, target);

        protected virtual void Add(IDirectBlendTreeItem tree, DirectBlendTree.Target target)
        {
            if (target == DirectBlendTree.Target.OFF)
                OFF = new BlendTreeOrMotion(tree);
            else if (target == DirectBlendTree.Target.ON)
                ON = new BlendTreeOrMotion(tree);
        }
    }
}
