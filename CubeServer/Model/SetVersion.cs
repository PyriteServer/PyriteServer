// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersion.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;

    public class SetVersion
    {
        public SetVersionLevelOfDetail[] DetailLevels { get; set; }
        public DateTime Loaded { get; set; }
        public Uri Material { get; set; }
        public string Name { get; set; }
        public Uri SourceUri { get; set; }
        public string Version { get; set; }
    }
}