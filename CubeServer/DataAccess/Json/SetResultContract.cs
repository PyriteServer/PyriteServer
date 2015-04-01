// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetResultContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess.Json
{
    using Newtonsoft.Json;

    public class SetResultContract
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}