// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Set.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;

    public class Set
    {
        public DateTime Loaded { get; set; }
        public string Name { get; set; }
        public Uri SourceUri { get; set; }
        public string Version { get; set; }
        public string MaterialName { get; set; }
        public SetVersionLevelOfDetail[] DetailLevels { get; set; }
    }
}