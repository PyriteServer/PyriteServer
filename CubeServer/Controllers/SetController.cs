// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using CubeServer.Contracts;

    public class SetController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}")]
        public IHttpActionResult Get(string setid)
        {
            try
            {
                var result = Dependency.Storage.EnumerateSetVersions(setid).OrderBy(version => version.Name);
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                return this.NotFound();
            }
        }

        [HttpGet]
        [Route("sets")]
        public IHttpActionResult GetAll()
        {
            IOrderedEnumerable<SetResultContract> result = Dependency.Storage.EnumerateSets().OrderBy(set => set.Name);
            return this.Ok(ResultWrapper.OkResult(result));
        }
    }
}