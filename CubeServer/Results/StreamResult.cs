// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="StreamResult.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Results
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using CubeServer.Contracts;

    public class StreamResult : IHttpActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly StorageStream _storageStream;

        public StreamResult(StorageStream storageStream, HttpRequestMessage request)
        {
            this._storageStream = storageStream;
            this._request = request;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(this._storageStream.Stream);
            response.Content.Headers.ContentLength = this._storageStream.Length;
            response.Content.Headers.ContentType = this._storageStream.TypeHeaderValue;
            response.RequestMessage = this._request;
            return response;
        }
    }
}