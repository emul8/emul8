//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//


namespace Emul8.Utilities.Collections
{
    public class TreeNode<T> : TreeBase<TreeNode<T>, T>
    {
        public TreeNode(T value) : base(value)
        {

        }

        public override TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value);
            ChildrenList.Add(node);
            return node;
        }
    }
}

