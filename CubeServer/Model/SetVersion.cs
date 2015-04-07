// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersion.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    public class SetVersion
    {
        public string Name { get; set; }
        public SetVersionLevelOfDetail[] DetailLevels { get; set; }
    }
}