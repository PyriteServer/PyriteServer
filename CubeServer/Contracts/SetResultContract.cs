// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetResultContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using System;
    using Newtonsoft.Json;

    public class SetResultContract
    {
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}