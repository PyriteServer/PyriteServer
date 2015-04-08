// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetMetadataContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess.Json
{
    public class SetMetadataContract
    {
        public int MinimumLod { get; set; }
        public int MaximumLod { get; set; }
        public string Mtl { get; set; }
    }
}