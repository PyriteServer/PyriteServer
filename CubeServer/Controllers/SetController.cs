// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;
    using System.Web.Http;

    public class SetController : ApiController
    {

        [HttpGet]
        [Route("sets")]
        public IHttpActionResult GetAll()
        {
            return this.Ok(Dependency.Storage.EnumerateSets());
        }

        [HttpGet]
        [Route("sets/{setid}")]
        public IHttpActionResult Get(string setid)
        {
            try
            {
                return this.Ok(Dependency.Storage.EnumerateSetVersions(setid));
            }
            catch (NotFoundException ex)
            {
                return this.NotFound();
            }
        }
    }
}