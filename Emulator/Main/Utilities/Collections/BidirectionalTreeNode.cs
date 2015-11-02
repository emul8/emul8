//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Utilities.Collections
{
    public class BidirectionalTreeNode<T> : TreeBase<BidirectionalTreeNode<T>, T>
    {
        public BidirectionalTreeNode(T value) : base(value)
        {
            // parent will be null - like for the root node
        }

        private BidirectionalTreeNode(T value, BidirectionalTreeNode<T> parent) : base(value)
        {
            Parent = parent;
        }

        public override BidirectionalTreeNode<T> AddChild(T value)
        {
            var node = new BidirectionalTreeNode<T>(value, this);
            ChildrenList.Add(node);
            return node;
        }

        public BidirectionalTreeNode<T> Parent { get; private set; }
    }
}

