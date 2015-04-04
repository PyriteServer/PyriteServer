// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersion.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    public class SetVersion
    {
        public OcTree<CubeBounds> Cubes { get; set; }
        public int Number { get; set; }
        public string CubePathFormat { get; set; }
    }
}