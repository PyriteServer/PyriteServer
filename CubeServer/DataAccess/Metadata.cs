// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Metadata.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    public class Metadata
    {
        public bool[][][] CubeExists { get; set; }

        public Extents Extents { get; set; }

        public GridSize GridSize { get; set; }
    }
}