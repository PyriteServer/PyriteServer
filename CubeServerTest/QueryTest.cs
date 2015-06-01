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

        [TestMethod]
        public void LargeDataSetQueryPerformanceCube()
        {
            SetVersion setVersion = this.GetLargeDataSet();

            SetVersionLevelOfDetail lod0 = setVersion.DetailLevels.Values.FirstOrDefault();

            Stopwatch timer = new Stopwatch();
            timer.Start();
            int[][] results = setVersion.Query("L1", lod0.WorldBounds).ToArray();
            Trace.WriteLine(results.Count(), "Elements");
            timer.Stop();
            Trace.WriteLine(timer.ElapsedMilliseconds, "Query Performance (ms)");
        }

        [TestMethod]
        public void RubiksCubeDataValidationOffset()
        {
            SetVersion rubikSetVersion = this.GenerateRubiksCubeLOD(new[] { 27, 9, 3 }, new Vector3(27, 27, 27));
            IEnumerable<QueryDetailContract> results = rubikSetVersion.Query("L2", new Vector3(13, 14, 13));
            Assert.AreEqual(3, results.Count());

            foreach (QueryDetailContract lod in results)
            {
                Trace.WriteLine(lod.Name, "Level of Detail Name");
                Assert.IsTrue(lod.Cubes.Count() <= 27);

                var allCubes = rubikSetVersion.DetailLevels[lod.Name].Cubes.AllItems();
                var lookup = allCubes.ToDictionary(c => c.BoundingBox.Min.X + "," + c.BoundingBox.Min.Y + "," + c.BoundingBox.Min.Z);

                Trace.WriteLine(lookup.Count, "Lookups");

                var cubes = lod.Cubes.ToArray();

                foreach (var key in cubes)
                {
                    var keyString = key[0] + "," + key[1] + "," + key[2];
                    Trace.WriteLine(keyString);
                    CubeBounds hit;
                    Assert.IsTrue(lookup.TryGetValue(keyString, out hit));
                    Assert.IsInstanceOfType(hit, typeof(ValidCube));
                }
            }
        }

        [TestMethod]
        public void RubiksCubeDataValidationCentered()
        {
            SetVersion rubikSetVersion = this.GenerateRubiksCubeLOD(new[] { 27, 9, 3 }, new Vector3(27, 27, 27));
            IEnumerable<QueryDetailContract> results = rubikSetVersion.Query("L2", new Vector3(13, 13, 13));
            Assert.AreEqual(3, results.Count());

            foreach (QueryDetailContract lod in results)
            {
                Trace.WriteLine(lod.Name, "Level of Detail Name");
                Assert.IsTrue(lod.Cubes.Count() <= 27);

                var allCubes = rubikSetVersion.DetailLevels[lod.Name].Cubes.AllItems();
                var lookup = allCubes.ToDictionary(c => c.BoundingBox.Min.X + "," + c.BoundingBox.Min.Y + "," + c.BoundingBox.Min.Z);

                Trace.WriteLine(lookup.Count, "Lookups");

                var cubes = lod.Cubes.ToArray();

                foreach (var key in cubes)
                {
                    var keyString = key[0] + "," + key[1] + "," + key[2];
                    Trace.WriteLine(keyString);
                    CubeBounds hit;
                    Assert.IsTrue(lookup.TryGetValue(keyString, out hit));
                    Assert.IsInstanceOfType(hit, typeof(ValidCube));
                }
            }
        }

        [TestMethod]
        public void TestBoundingBox()
        {
            SetVersion setVersion = this.Get4x4x4DataSet1();
            int[][] results = setVersion.Query("L1", new BoundingBox(new Vector3(10, 10, 10), new Vector3(20, 20, 20))).ToArray();
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual("0,0,0", results.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First());
        }

        private SetVersionLevelOfDetail GenerateRubikLevelOfDetail(string name, int scale, Vector3 worldBounds)
        {
            int offset = scale / 3;
            int min = offset;
            int max = (2 * offset);
            List<CubeBounds> testBounds = new List<CubeBounds>();
            SetVersionLevelOfDetail lod;

            for (int x = 0; x < scale; x++)
            {
                for (int y = 0; y < scale; y++)
                {
                    for (int z = 0; z < scale; z++)
                    {
                        if (offset != 1)
                        {
                            if ((min <= x && x < max) && (min <= y && y < max) && (min <= z && z < max))
                            {
                                testBounds.Add(new ValidCube { BoundingBox = this.MakeCube(new Vector3(x, y, z), 1) });
                            }
                            else
                            {
                                testBounds.Add(new InvalidCube { BoundingBox = this.MakeCube(new Vector3(x, y, z), 1) });
                            }
                        }
                        else
                        {
                            testBounds.Add(new ValidCube { BoundingBox = this.MakeCube(new Vector3(x, y, z), 1) });
                        }
                    }
                }
            }
            OcTree<CubeBounds> ocTree = new OcTree<CubeBounds>(this.zeroBoundingBox, testBounds);
            ocTree.UpdateTree();
            lod = new SetVersionLevelOfDetail
                  {
                      Name = name,
                      Cubes = ocTree,
                      SetSize = new Vector3(scale,scale,scale),
                      WorldBounds = new BoundingBox(Vector3.Zero, worldBounds)
                  };
            return lod;
        }

        private SetVersion GenerateRubiksCubeLOD(IEnumerable<int> scales, Vector3 worldBounds)
        {
            SetVersion setVersion = new SetVersion();
            setVersion.Name = "RubiksCubeData";
            setVersion.Version = "v1";

            List<SetVersionLevelOfDetail> lods = new List<SetVersionLevelOfDetail>();

            int index = 1;

            foreach (int scale in scales)
            {
                SetVersionLevelOfDetail lod = this.GenerateRubikLevelOfDetail("L" + index++, scale, worldBounds);
                lods.Add(lod);
            }

            setVersion.DetailLevels =
                new SortedDictionary<string, SetVersionLevelOfDetail>(lods.ToDictionary(l => l.Name, l => l, StringComparer.OrdinalIgnoreCase));

            return setVersion;
        }

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
                                              WorldBounds = new BoundingBox(Vector3.Zero, new Vector3(40, 40, 40))
                                          };

            setVersion.DetailLevels =
                new SortedDictionary<string, SetVersionLevelOfDetail>(new[] { lod }.ToDictionary(l => l.Name, l => l, StringComparer.OrdinalIgnoreCase));
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
                ocTree = MetadataLoader.Load(metadataStream, ocTree, "L1", new Vector3(1, 1, 1));
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
                                              WorldBounds = new BoundingBox(Vector3.Zero, new Vector3(40, 40, 40))
                                          };

            setVersion.DetailLevels =
                new SortedDictionary<string, SetVersionLevelOfDetail>(new[] { lod }.ToDictionary(l => l.Name, l => l, StringComparer.OrdinalIgnoreCase));
            return setVersion;
        }

        private BoundingBox MakeCube(Vector3 min, float size)
        {
            Vector3 max = new Vector3 { X = min.X + size, Y = min.Y + size, Z = min.Z + size };
            return new BoundingBox { Min = min, Max = max };
        }

        public class InvalidCube : CubeBounds
        {
        }

        public class ValidCube : CubeBounds
        {
        }
    }
}