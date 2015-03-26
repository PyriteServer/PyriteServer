// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="OctTreeTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System;
    using System.Collections.Generic;
    using CubeServer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass]
    public class OctTreeTest
    {
        private readonly BoundingBox zeroBoundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
        private readonly BoundingBox oneBoundingBox = new BoundingBox(Vector3.Zero, Vector3.One);

        [TestMethod]
        public void Initialize()
        {
            OctTree<TestBounds> testOctTree = new OctTree<TestBounds>();
            Assert.IsFalse(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.AreEqual(zeroBoundingBox, testOctTree.Region);
        }

        [TestMethod]
        public void InitializeWithRegion()
        {
            OctTree<TestBounds> testOctTree = new OctTree<TestBounds>(oneBoundingBox);
            Assert.IsFalse(testOctTree.HasChildren);
            Assert.IsTrue(testOctTree.IsRoot);
            Assert.IsNotNull(testOctTree.Region);
            Assert.AreEqual(Vector3.One, testOctTree.Region.Max);
        }

        [TestMethod]
        public void InitializeWithRegionAndData()
        {
            List<TestBounds> testBounds = new List<TestBounds>();

            testBounds.Add(new TestBounds{BoundingBox = oneBoundingBox});
            testBounds.Add(new TestBounds{BoundingBox = new BoundingBox(new Vector3(2,0,0), new Vector3(3,1,0))});

            OctTree<TestBounds> testOctTree = new OctTree<TestBounds>(zeroBoundingBox, testBounds);

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
        }
    }

    internal class TestBounds : IBounds<TestBounds>
    {
        public BoundingBox BoundingBox { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        public Intersection<TestBounds> Intersects(Ray ray)
        {
            throw new NotImplementedException();
        }

        public Intersection<TestBounds> Intersects(TestBounds obj)
        {
            Intersection<TestBounds> ir;

            if (obj.BoundingBox.Min != obj.BoundingBox.Max)
            {
                ir = Intersects(obj.BoundingBox);
            }
            else if (obj.BoundingSphere.Radius != 0f)
            {
                ir = Intersects(obj.BoundingSphere);
            }
            else
                return null;

            if (ir != null)
            {
                ir.Object = this;
                ir.OtherObject = obj;
            }

            return ir;
        }

        public Intersection<TestBounds> Intersects(BoundingSphere intersectionSphere)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                if (this.BoundingBox.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new Intersection<TestBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new Intersection<TestBounds>(this);
            }

            return null;
        }

        public Intersection<TestBounds> Intersects(BoundingBox intersectionBox)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                ContainmentType ct = this.BoundingBox.Contains(intersectionBox);
                if (ct != ContainmentType.Disjoint)
                    return new Intersection<TestBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Contains(intersectionBox) != ContainmentType.Disjoint)
                    return new Intersection<TestBounds>(this);
            }

            return null;
        }

        public Intersection<TestBounds> Intersects(BoundingFrustum frustum)
        {
            throw new NotImplementedException();
        }
    }
}