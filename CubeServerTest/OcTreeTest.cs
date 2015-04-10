// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OctTreeTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System.Collections.Generic;
    using CubeServer;
    using CubeServer.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass]
    public class OcTreeTest
    {
        private readonly BoundingBox oneBoundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
        private readonly BoundingBox zeroBoundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

        [TestMethod]
        public void Initialize()
        {
            OcTree<CubeBounds> testOcTree = new OcTree<CubeBounds>();
            Assert.IsFalse(testOcTree.HasChildren);
            Assert.IsTrue(testOcTree.IsRoot);
            Assert.AreEqual(this.zeroBoundingBox, testOcTree.Region);
            OcTreeUtilities.Dump(testOcTree);
        }

        [TestMethod]
        public void InitializeWithRegion()
        {
            OcTree<CubeBounds> testOcTree = new OcTree<CubeBounds>(this.oneBoundingBox);
            Assert.IsFalse(testOcTree.HasChildren);
            Assert.IsTrue(testOcTree.IsRoot);
            Assert.IsNotNull(testOcTree.Region);
            Assert.AreEqual(Vector3.One, testOcTree.Region.Max);
            OcTreeUtilities.Dump(testOcTree);
        }

        [TestMethod]
        public void InitializeWithRegionAndData()
        {
            List<CubeBounds> testBounds = new List<CubeBounds>();

            testBounds.Add(new CubeBounds { BoundingBox = this.oneBoundingBox });
            testBounds.Add(new CubeBounds { BoundingBox = new BoundingBox(new Vector3(2, 0, 0), new Vector3(3, 1, 0)) });

            OcTree<CubeBounds> testOcTree = new OcTree<CubeBounds>(this.zeroBoundingBox, testBounds);

            Assert.IsFalse(testOcTree.HasChildren);
            Assert.IsTrue(testOcTree.IsRoot);
            Assert.IsNotNull(testOcTree.Region);
            Assert.AreEqual(Vector3.Zero, testOcTree.Region.Max);

            testOcTree.UpdateTree();

            Assert.IsTrue(testOcTree.HasChildren);
            Assert.IsTrue(testOcTree.IsRoot);
            Assert.IsNotNull(testOcTree.Region);
            Assert.AreEqual(Vector3.Zero, testOcTree.Region.Min);
            Assert.AreEqual(new Vector3(4, 4, 4), testOcTree.Region.Max);

            Assert.AreEqual(0, testOcTree.Objects.Count);

            OcTreeUtilities.Dump(testOcTree);
        }
    }
}