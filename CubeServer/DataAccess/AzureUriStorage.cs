// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="AzureUriStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;

    public class AzureUriStorage : UriStorage
    {
        private readonly string sasHostName;

        public AzureUriStorage(string sasHostName, string storageRootUri) : base(storageRootUri)
        {
            this.sasHostName = sasHostName;
        }

        protected override void TransformUri(Uri sourceUri)
        {
            if (sourceUri.Host != this.sasHostName)
            {
                return;
            }

            // generate SAS URL
        }
    }
}