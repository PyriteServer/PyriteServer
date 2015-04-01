// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="FileCubeStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Security;
    using System.Threading.Tasks;
    using System.Web;
    using CubeServer.Contracts;
    using CubeServer.DataAccess.Json;

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
                yield return new SetResultContract { Name = info.Name};
            }
        }

        public IEnumerable<VersionResultContract> EnumerateSetVersions(string setid)
        {
            string setPath = Path.Combine(this.storageRootDirectory, setid);

            if (Path.GetDirectoryName(setPath) != this.storageRootDirectory)
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
                DirectoryInfo info = new DirectoryInfo(directory);
                yield return new VersionResultContract { Name = info.Name};
            }
        }

        public Task<StorageStream> GetTextureStream(string setId, string version, string detail, string textureid)
        {
            string texturePath = Path.Combine(this.storageRootDirectory, setId, detail, textureid + ".jpg");
            if (!File.Exists(texturePath))
            {
                throw new NotFoundException(texturePath);
            }

            FileInfo info = new FileInfo(texturePath);
            FileStream fs = File.Open(texturePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(new StorageStream(fs, info.Length, new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(Path.GetExtension(texturePath)))));
        }
    }
}