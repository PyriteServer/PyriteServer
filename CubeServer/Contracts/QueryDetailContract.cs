// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryDetailContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class QueryDetailContract
    {
        [JsonProperty("cubes")]
        public IEnumerable<int[]> Cubes { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}