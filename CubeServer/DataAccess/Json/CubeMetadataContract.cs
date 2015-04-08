// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Metadata.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess.Json
{
    using Microsoft.Xna.Framework;

    public class CubeMetadataContract
    {
        public bool[][][] CubeExists { get; set; }

        public ExtentsContract WorldBounds { get; set; }

        public Vector3 SetSize { get; set; }

        public Vector2 TextureSetSize { get; set; }
    }
}