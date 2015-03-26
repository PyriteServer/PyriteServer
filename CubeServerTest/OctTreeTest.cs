// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OctTreeTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using CubeServer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass]
    public class OctTreeTest
    {
        private readonly BoundingBox oneBoundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
        private readonly BoundingBox zeroBoundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        private readonly OctTreeUtilities octTreeUtilities = new OctTreeUtilities();

        [TestMethod]
        public void Initialize()
        {
            OctTree<CubeBounds> testOctTree = new OctTree<CubeBounds>();
            Assert.IsFalse(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.AreEqual(this.zeroBoundingBox, testOctTree.Region);
            OctTreeUtilities.Dump(testOctTree);
        }

        [TestMethod]
        public void InitializeWithRegion()
        {
            OctTree<CubeBounds> testOctTree = new OctTree<CubeBounds>(this.oneBoundingBox);
            Assert.IsFalse(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.IsNotNull(testOctTree.Region);
            Assert.AreEqual(Vector3.One, testOctTree.Region.Max);
            OctTreeUtilities.Dump(testOctTree);
        }

        [TestMethod]
        public void InitializeWithRegionAndData()
        {
            List<CubeBounds> testBounds = new List<CubeBounds>();

            testBounds.Add(new CubeBounds { BoundingBox = this.oneBoundingBox });
            testBounds.Add(new CubeBounds { BoundingBox = new BoundingBox(new Vector3(2, 0, 0), new Vector3(3, 1, 0)) });

            OctTree<CubeBounds> testOctTree = new OctTree<CubeBounds>(this.zeroBoundingBox, testBounds);

            Assert.IsFalse(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.IsNotNull(testOctTree.Region);
            Assert.AreEqual(Vector3.Zero, testOctTree.Region.Max);

            testOctTree.UpdateTree();

            Assert.IsTrue(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.IsNotNull(testOctTree.Region);
            Assert.AreEqual(Vector3.Zero, testOctTree.Region.Min);
            Assert.AreEqual(new Vector3(4, 4, 4), testOctTree.Region.Max);

            Assert.AreEqual(0, testOctTree.Objects.Count);

            OctTreeUtilities.Dump(testOctTree);
        }
    }
}