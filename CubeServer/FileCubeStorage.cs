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
        private readonly string storageRootDirectory;

        public FileCubeStorage(string storageRoot)
        {
            if (storageRoot == null)
            {
                throw new ArgumentNullException("storageRoot");
            }

            string storageFullPath = Path.GetFullPath(storageRoot);

            if (!Directory.Exists(storageFullPath))
            {
                Directory.CreateDirectory(storageRoot);
            }

            this.storageRootDirectory = storageRoot;
        }

        public IEnumerable<string> EnumerateSets()
        {
            string[] childDirectories = Directory.GetDirectories(this.storageRootDirectory);
            foreach (string directory in childDirectories)
            {
                string name = Path.GetFileName(directory);
                yield return name;
            }
        }
    }
}