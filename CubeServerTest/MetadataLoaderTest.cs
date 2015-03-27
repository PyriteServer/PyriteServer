// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="MetadataLoaderTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System.IO;
    using CubeServer;
    using CubeServer.DataAccess;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass]
    [DeploymentItem(@"data\", "data")]
    public class MetadataLoaderTest
    {
        [TestMethod]
        public void LoadDefaultMinSize()
        {
            OctTree<CubeBounds> octTree;
            MetadataLoader loader = new MetadataLoader();
            using (Stream metadataStream = new FileStream(".\\data\\validdataset1\\v1\\metadata.json", FileMode.Open, FileAccess.Read))
            {
                octTree = loader.Load(metadataStream);
            }

            octTree.UpdateTree();
            Assert.IsNotNull(octTree);
            Assert.IsTrue(octTree.HasChildren);

            OctTreeUtilities.Dump(octTree);
        }

        [TestMethod]
        public void LoadDefaultSize2()
        {
            OctTree<CubeBounds> octTree = new OctTree<CubeBounds>(new BoundingBox(Vector3.Zero, Vector3.Zero), new CubeBounds[]{}, 2);
            MetadataLoader loader = new MetadataLoader();
            using (Stream metadataStream = new FileStream(".\\data\\validdataset1\\v1\\metadata.json", FileMode.Open, FileAccess.Read))
            {
                octTree = loader.Load(metadataStream, octTree);
            }

            octTree.UpdateTree();
            Assert.AreEqual(2, octTree.MinimumSize);
            Assert.IsNotNull(octTree);
            Assert.IsTrue(octTree.HasChildren);

            OctTreeUtilities.Dump(octTree);   
        }
    }
}