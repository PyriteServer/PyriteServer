// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ICubeStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using System.Collections.Generic;

    public interface ICubeStorage
    {
        IEnumerable<SetResultContract> EnumerateSets();

        IEnumerable<VersionResultContract> EnumerateSetVersions(string setId);
    }
}