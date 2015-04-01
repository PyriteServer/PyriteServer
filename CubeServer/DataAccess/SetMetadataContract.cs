// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetMetadataContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    public class SetMetadataContract
    {
        public string CubeTemplate { get; set; }
        public string JpgTemplate { get; set; }
        public int MaximumViewport { get; set; }
        public string MetadataTemplate { get; set; }
        public int MinimumViewport { get; set; }
        public string MtlTemplate { get; set; }
        public string TexturePath { get; set; }
        public int TextureSubdivide { get; set; }
    }
}