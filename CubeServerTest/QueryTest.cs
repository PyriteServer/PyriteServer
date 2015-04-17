// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using CubeServer.Contracts;
    using CubeServer.DataAccess;
    using CubeServer.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass]
    [DeploymentItem(@"data\", "data")]

    public class QueryTest
    {
        private readonly BoundingBox oneBoundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
        private readonly BoundingBox zeroBoundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

        private SetVersion Get4x4x4DataSet1()
        {
            SetVersion setVersion = new SetVersion();
            setVersion.Name = "Test";
            setVersion.Version = "v1";

            List<CubeBounds> testBounds = new List<CubeBounds>();

            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(Vector3.Zero, 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(1, 1, 1), 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(2, 2, 2), 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(3, 3, 3), 1) });

            OcTree<CubeBounds> ocTree = new OcTree<CubeBounds>(this.zeroBoundingBox, testBounds);
            ocTree.UpdateTree();

            SetVersionLevelOfDetail lod = new SetVersionLevelOfDetail
                                          {
                                              Name = "L1",
                                              Cubes = ocTree,
                                              SetSize = ocTree.Region.Max - ocTree.Region.Min,
                                              VirtualWorldBounds = new BoundingBox(Vector3.Zero, new Vector3(40, 40, 40))
                                          };

            setVersion.DetailLevels = new[] { lod };
            return setVersion;
        }

        private SetVersion GetLargeDataSet()
        {
            SetVersion setVersion = new SetVersion();
            setVersion.Name = "Test";
            setVersion.Version = "v1";

            OcTree<CubeBounds> ocTree = new OcTree<CubeBounds>(new BoundingBox(Vector3.Zero, Vector3.Zero), new CubeBounds[] { }, 2);
            using (Stream metadataStream = new FileStream(".\\data\\validdataset2\\v1\\metadata.json", FileMode.Open, FileAccess.Read))
            {
                ocTree = MetadataLoader.Load(metadataStream, ocTree);
            }

            ocTree.UpdateTree();
            Assert.AreEqual(2, ocTree.MinimumSize);
            Assert.IsNotNull(ocTree);
            Assert.IsTrue(ocTree.HasChildren);

            SetVersionLevelOfDetail lod = new SetVersionLevelOfDetail
            {
                Name = "L1",
                Cubes = ocTree,
                SetSize = ocTree.Region.Max - ocTree.Region.Min,
                VirtualWorldBounds = new BoundingBox(Vector3.Zero, new Vector3(40, 40, 40))
            };

            setVersion.DetailLevels = new[] { lod };
            return setVersion;
        }

        [TestMethod]
        public void TestBoundingBox()
        {
            SetVersion setVersion = this.Get4x4x4DataSet1();
            int[][] results = setVersion.Query("L1", new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20))).ToArray();
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual("0,0,0", results.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First());
        }

        [TestMethod]
        public void TestRubik1()
        {
            SetVersion setVersion = this.Get4x4x4DataSet1();
            QueryDetailContract[] results = setVersion.Query(new Vector3(15, 15, 15)).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(3, results[0].Cubes.Count());
            Assert.AreEqual("0,0,0", results[0].Cubes.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First());
        }

        [TestMethod]
        public void TestRubik2()
        {
            SetVersion setVersion = this.Get4x4x4DataSet1();
            QueryDetailContract[] results = setVersion.Query(new Vector3(25, 25, 25)).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(3, results[0].Cubes.Count());
            Assert.AreEqual("1,1,1", results[0].Cubes.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First());
        }

        private BoundingBox MakeCube(Vector3 min, float size)
        {
            Vector3 max = new Vector3 { X = min.X + size, Y = min.Y + size, Z = min.Z + size };
            return new BoundingBox { Min = min, Max = max };
        }

        [TestMethod]
        public void LargeDataSetQueryPerformanceRubiksCube()
        {
            SetVersion setVersion = this.GetLargeDataSet();

            var lod0 = setVersion.DetailLevels[0];

            Stopwatch timer = new Stopwatch();
            timer.Start();
            var results = setVersion.Query("L1", lod0.VirtualWorldBounds).ToArray();
            Trace.WriteLine(results.Count(), "Elements");
            timer.Stop();
            Trace.WriteLine(timer.ElapsedMilliseconds, "Query Performance (ms)");
        }
    }
}