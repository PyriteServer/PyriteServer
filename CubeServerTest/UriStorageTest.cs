// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UriStorageTest.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using CubeServer;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [DeploymentItem(@"data\", "data")]
    public class UriStorageTest
    {
        [TestMethod]
        public async Task Initialization()
        {
            string setsJson = Path.Combine(".", "data", "sets.json");
            setsJson = Path.GetFullPath(setsJson);
            Uri setsJsonUri = new Uri(setsJson);

            UriStorage storage = new UriStorage(setsJsonUri.ToString());
            LoaderResults results = await storage.Load();

            Assert.AreEqual(0, results.Errors.Length);
            Assert.AreEqual(2, results.Sets.Length);
        }
    }
}