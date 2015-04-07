// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="VersionController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Http;
    using CubeServer.Contracts;
    using CubeServer.DataAccess;
    using CubeServer.Model;

    public class VersionController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{versionid}")]
        public IHttpActionResult Get(string setid, string versionid)
        {
            try
            {
                SetVersionResultContract result = Dependency.Storage.GetSetVersion(setid, versionid);
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                Trace.WriteLine(ex, "SetController::Get");
                return this.NotFound();
            }
        }
    }
}