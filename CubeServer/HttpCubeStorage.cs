// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="HttpCubeStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CubeServer.Contracts;
    using CubeServer.DataAccess;
    using CubeServer.DataAccess.Json;

    public class HttpCubeStorage : ICubeStorage
    {
        private const string BASE_STORAGE_URL = "<BASE_STORAGE_URL>";
        // 0 - storage url
        // 1 - setid
        // 2 - detail id
        // 3 - textureid
        private const string TEXTURE_STORAGE_FORMAT = "{0}/{1}/{2}/{3}.jpg";

        public IEnumerable<SetResultContract> EnumerateSets()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<VersionResultContract> EnumerateSetVersions(string setId)
        {
            throw new NotImplementedException();
        }

        public async Task<StorageStream> GetTextureStream(string setId, string version, string detail, string textureid)
        {
            string texturePath = string.Format(TEXTURE_STORAGE_FORMAT, BASE_STORAGE_URL, setId, detail, textureid);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage storageResponseMessage = await client.GetAsync(new Uri(texturePath), HttpCompletionOption.ResponseHeadersRead);
                if (storageResponseMessage.IsSuccessStatusCode)
                {
                    return new StorageStream(
                        await storageResponseMessage.Content.ReadAsStreamAsync(),
                        storageResponseMessage.Content.Headers.ContentLength.Value,
                        storageResponseMessage.Content.Headers.ContentType);
                }
                else
                {
                    if (storageResponseMessage.StatusCode != HttpStatusCode.NotFound)
                    {
                        storageResponseMessage.EnsureSuccessStatusCode();
                    }

                    throw new NotFoundException(texturePath);
                }
            }
        }
    }
}