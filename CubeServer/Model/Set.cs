// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Set.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
    using Microsoft.Xna.Framework;

    public class Set
    {
        public DateTime Loaded { get; set; }
        public string Name { get; set; }
        public Uri SourceUri { get; set; }
        public SetVersion[] Versions { get; set; }
        public Vector2 TextureDivisions { get; set; }
        public string TexturePathFormat { get; set; }
    }
}