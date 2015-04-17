using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CubeServerTest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CubeServer;
    using CubeServer.Contracts;
    using CubeServer.DataAccess.Json;
    using CubeServer.Model;
    using Microsoft.Xna.Framework;

    [TestClass]
    public class QueryTest
    {
        private readonly BoundingBox oneBoundingBox = new BoundingBox(Vector3.Zero, Vector3.One);
        private readonly BoundingBox zeroBoundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

        [TestMethod]
        public void TestRubik1()
        {
            SetVersion setVersion = this.Get4x4x4();
            var results = setVersion.Query(new Vector3(15,15,15)).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(3, results[0].Cubes.Count());
            Assert.AreEqual("0,0,0", results[0].Cubes.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First()); 
        }

        [TestMethod]
        [Ignore]
        public void TestRubik2()
        {
            SetVersion setVersion = this.Get4x4x4();
            var results = setVersion.Query(new Vector3(25, 25, 25)).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(3, results[0].Cubes.Count());
            Assert.AreEqual("1,1,1", results[0].Cubes.Select(i => String.Format("{0},{1},{2}", i[0], i[1], i[2])).First());
        }

        public SetVersion Get4x4x4()
        {
            SetVersion setVersion = new SetVersion();
            setVersion.Name = "Test";
            setVersion.Version = "v1";

            List<CubeBounds> testBounds = new List<CubeBounds>();

            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(Vector3.Zero, 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(1, 1, 1), 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(2, 2, 2), 1) });
            testBounds.Add(new CubeBounds { BoundingBox = this.MakeCube(new Vector3(3, 3, 3), 1) });

            var ocTree = new OcTree<CubeBounds>(this.zeroBoundingBox, testBounds);
            ocTree.UpdateTree();

            SetVersionLevelOfDetail lod = new SetVersionLevelOfDetail
                                          {
                                              Name = "L1",
                                              Cubes = ocTree,
                                              SetSize = ocTree.Region.Max - ocTree.Region.Min,
                                              VirtualWorldBounds = new BoundingBox(Vector3.Zero, new Vector3(40,40,40))
                                          };

            setVersion.DetailLevels = new []{ lod };
            return setVersion;
        }
   
        private BoundingBox MakeCube(Vector3 min, float size)
        {
            Vector3 max = new Vector3 { X = min.X + size, Y = min.Y + size, Z = min.Z + size };
            return new BoundingBox { Min = min, Max = max };
        }
    }

}
