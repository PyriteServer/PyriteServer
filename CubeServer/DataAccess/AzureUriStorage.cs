// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="AzureUriStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>Provide access to Cubes stored securely in Azure Blob</summary>
    /// <remarks>Generates SAS uri's where host name requested matches configured blob store</remarks>
    public class AzureUriStorage : UriStorage
    {
        private readonly TimeSpan accessTimespan = TimeSpan.FromMinutes(30);
        private readonly CloudBlobClient client;
        private readonly string sasHost;

        public AzureUriStorage(String connectionString, string storageRootUri) : base(storageRootUri)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            this.client = account.CreateCloudBlobClient();

            this.sasHost = this.client.BaseUri.Host;
        }

        protected override Uri TransformUri(Uri sourceUri)
        {
            if (sourceUri.Host != this.sasHost)
            {
                return sourceUri;
            }

            // generate SAS URL
            ICloudBlob blobRef = this.client.GetBlobReferenceFromServer(sourceUri);
            string queryString = blobRef.GetSharedAccessSignature(this.GetReadPolicy());

            return new Uri(sourceUri.ToString() + queryString);
        }

        private SharedAccessBlobPolicy GetReadPolicy()
        {
            DateTimeOffset now = DateTimeOffset.Now;

            return new SharedAccessBlobPolicy
                   {
                       Permissions = SharedAccessBlobPermissions.Read,
                       SharedAccessStartTime = now - this.accessTimespan,
                       SharedAccessExpiryTime = now + this.accessTimespan
                   };
        }
    }
}