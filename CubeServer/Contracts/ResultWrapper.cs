// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ResultWrapper.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Contracts
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ResultWrapper
    {
        public enum ResultStatus
        {
            OK,
            ERROR
        }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public string Message { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public Object Result { get; set; }

        [JsonProperty("status", Order = 1)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultStatus Status { get; set; }

        public static ResultWrapper OkResult(Object payload)
        {
            return new ResultWrapper { Result = payload, Status = ResultStatus.OK };
        }
    }
}