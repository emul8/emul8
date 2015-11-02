//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Utilities.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTests.Collections
{
	[TestFixture]
	public class MultiTreeTests
	{
		[Test]
		public void ShouldTraverseSimpleTree()
		{
			var tree = new MultiTree<int, string>(1);
			tree.AddChild(2);
			tree.AddChild(3);
			CollectionAssert.AreEquivalent(new [] { 1, 2, 3 }, tree);
		}

		[Test]
		public void ShouldTraverseAnotherSimpleTree()
		{
			var tree = new MultiTree<int, string>(1);
			var twoNode = tree.AddChild(2);
			twoNode.AddChild(3);
			twoNode.AddChild(4);
			tree.AddChild(5);
			CollectionAssert.AreEquivalent(new [] { 1, 2, 3, 4, 5 }, tree);
		}

		[Test]
		public void ShouldTraverseMultiTree()
		{
			var tree = new MultiTree<int, string>(1);
			var twoNode = tree.AddChild(2);
			twoNode.AddChild(3);
			tree.AddChild(3);
			CollectionAssert.AreEquivalent(new [] { 1, 2, 3, 3 }, tree);
		}

		[Test]
		public void ShouldCreateOneNodeForTheSameValue()
		{
			var tree = new MultiTree<int, string>(1);
			var twoNode = tree.AddChild(2);
			var threeNode = twoNode.AddChild(3);
			var anotherTreeNode = tree.AddChild(3);
			Assert.AreSame(threeNode, anotherTreeNode);
		}

		[Test]
		public void ShouldFindRoot()
		{
			var tree = new MultiTree<int, string>(1);
			Assert.AreEqual(1, tree.GetNode(1).Value);
		}

		[Test]
		public void ShouldRemoveSubtree()
		{
			var tree = new MultiTree<int, string>(1);
			var twoNode = tree.AddChild(2);
			twoNode.AddChild(3);
			twoNode.AddChild(4);
			tree.AddChild(5);
			tree.RemoveChild(2);
			CollectionAssert.AreEquivalent(new [] { 1, 5 }, tree);
		}

		[Test]
		public void ShouldRemoveSubtreeFromDictionary()
		{
			var tree = new MultiTree<int, string>(1);
			var twoNode = tree.AddChild(2);
			twoNode.AddChild(3);
			twoNode.AddChild(4);
			tree.AddChild(5);
			tree.RemoveChild(2);
			Assert.Throws(typeof(KeyNotFoundException), () => tree.GetNode(2));
			Assert.Throws(typeof(KeyNotFoundException), () => tree.GetNode(3));
			Assert.Throws(typeof(KeyNotFoundException), () => tree.GetNode(4));
			tree.GetNode(5);
		}
	}
}

