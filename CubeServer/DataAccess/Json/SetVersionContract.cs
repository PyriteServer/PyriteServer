// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersionContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess.Json
{
    using Newtonsoft.Json;

    public class SetVersionContract
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

}