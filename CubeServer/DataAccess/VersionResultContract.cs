// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="VersionResultContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using Newtonsoft.Json;

    public class VersionResultContract
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}