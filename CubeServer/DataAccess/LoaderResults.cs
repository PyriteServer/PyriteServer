// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="LoaderResults.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System.Collections.Generic;
    using CubeServer.Model;

    public class LoaderResults
    {
        public LoaderException[] Errors { get; set; }
        public IDictionary<string, Dictionary<string, SetVersion>> Sets { get; set; }
        public bool Success { get; set; }

        public SetVersion FindSetVersion(string setId, string versionId)
        {
            SetVersion setVersion;
            if (this.Sets == null)
            {
                throw new NotFoundException("set data");
            }

            Dictionary<string, SetVersion> versions;
            if (!this.Sets.TryGetValue(setId, out versions))
            {
                throw new NotFoundException("set");
            }

            if (!versions.TryGetValue(versionId, out setVersion))
            {
                throw new NotFoundException("version");
            }
            return setVersion;
        }
    }
}