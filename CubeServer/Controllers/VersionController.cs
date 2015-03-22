// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="VersionController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    public class VersionController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{versionid}")]
        public IEnumerable<string> Get(string setid, string versionid)
        {
            return new[] { "value1", "value2" };
        }
    }
}