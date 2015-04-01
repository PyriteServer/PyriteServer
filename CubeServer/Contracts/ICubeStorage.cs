// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ICubeStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CubeServer.DataAccess;

    public interface ICubeStorage
    {
        IEnumerable<SetResultContract> EnumerateSets();

        IEnumerable<VersionResultContract> EnumerateSetVersions(string setId);

        Task<StorageStream> GetTextureStream(string setId, string version, string detail, string textureid);
    }
}