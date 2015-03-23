// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Dependency.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using CubeServer.Contracts;

    public static class Dependency
    {
        public static ICubeStorage Storage { get; set; }
    }
}