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

    [TestClass]
    [DeploymentItem(@"data\", "data")]
    public class MetadataLoaderTest
    {
        [TestMethod]
        public void Load()
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
    }
}