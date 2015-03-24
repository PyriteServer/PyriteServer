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
    using System.Security;
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
                Directory.CreateDirectory(storageFullPath);
            }

            this.storageRootDirectory = storageFullPath;
        }

        public IEnumerable<SetResultContract> EnumerateSets()
        {
            string[] childDirectories = Directory.GetDirectories(this.storageRootDirectory);
            foreach (string directory in childDirectories)
            {
                DirectoryInfo info = new DirectoryInfo(directory);
                yield return new SetResultContract { Name = info.Name, CreationDate = info.CreationTimeUtc };
            }
        }

        public IEnumerable<string> EnumerateSetVersions(string setid)
        {
            string setPath = Path.Combine(this.storageRootDirectory, setid);

            if (Path.GetDirectoryName(setPath) != storageRootDirectory)
            {
                throw new SecurityException("Invalid set name");   
            }

            if (!Directory.Exists(setPath))
            {
                throw new NotFoundException(setPath);
            }

            string[] childDirectories = Directory.GetDirectories(setPath);

            foreach (string directory in childDirectories)
            {
                string name = Path.GetFileName(directory);
                yield return name;
            }
        }
    }
}