// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UnitTest1.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServerTest
{
    using System;
    using System.IO;
    using CubeServer;
    using CubeServer.Contracts;
    using CubeServer.DataAccess;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileCubeStorageTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeThrowsNullArg()
        {
            ICubeStorage fileStorage = new FileCubeStorage(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitializeThrowsInvalidDirectoryName()
        {
            ICubeStorage fileStorage = new FileCubeStorage("::");
        }

        [TestMethod]
        public void InitializeCreatesDirectory()
        {
            ICubeStorage fileStorage = new FileCubeStorage(".\\Test");
            Assert.IsTrue(Directory.Exists(".\\Test"));
        }
    }
}