// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersionResultContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using Newtonsoft.Json;

    public class SetVersionResultContract
    {
        [JsonProperty("detailLevels")]
        public LevelOfDetailContract[] DetailLevels { get; set; }

        [JsonProperty("set")]
        public string Set { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}