// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="FileCubeStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CubeServer.Contracts;

    public class FileCubeStorage : ICubeStorage
    {
        private string storageRootDirectory;

        public FileCubeStorage(string storageRoot)
        {
            if (storageRoot == null)
            {
                throw new ArgumentNullException("storageRoot");
            }

            if (!Directory.Exists(storageRoot))
            {
                throw new DirectoryNotFoundException(storageRootDirectory);
            }

            this.storageRootDirectory = storageRoot;
        }

        public IEnumerable<string> EnumerateSets()
        {

        }
    }
}