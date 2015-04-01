// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Metadata.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess.Json
{
    public class CubeMetadataContract
    {
        public bool[][][] CubeExists { get; set; }

        public ExtentsContract Extents { get; set; }

        public GridSizeContract GridSize { get; set; }
    }
}