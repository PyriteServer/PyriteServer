// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersionLevelOfDetail.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
    using Microsoft.Xna.Framework;

    public class SetVersionLevelOfDetail
    {
        public OcTree<CubeBounds> Cubes { get; set; }
        public Uri Metadata { get; set; }
        public Uri TextureTemplate { get; set; }
        public Uri ModelTemplate { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public Vector3 SetSize { get; set; }
        public Vector2 TextureSetSize { get; set; }
        public BoundingBox WorldBounds { get; set; }
    }
}