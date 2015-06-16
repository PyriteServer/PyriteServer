// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryResultContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Contracts
{
    using Newtonsoft.Json;

    public class QueryResultContract
    {
        [JsonProperty("v3")]
        public string V3 { get; set; }
    }
}