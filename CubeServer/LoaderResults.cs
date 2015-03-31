// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="LoaderResults.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    public class LoaderResults
    {
        public LoaderException[] Errors { get; set; }
        public Set[] Sets { get; set; }
        public bool Success { get; set; }
    }
}