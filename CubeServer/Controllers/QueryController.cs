// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Http;
    using CubeServer.Contracts;
    using Microsoft.Xna.Framework;

    public class QueryController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{versionId}/query/{detail}/{ax},{ay},{az}/{bx},{by},{bz}")]
        public IHttpActionResult BoundingBoxQuery(string setId, string versionId, string detail, float ax, float ay, float az, float bx, float by, float bz)
        {
            try
            {
                IEnumerable<int[]> result = Dependency.Storage.Query(
                    setId,
                    versionId,
                    detail,
                    new BoundingBox(new Vector3(ax, ay, az), new Vector3(bx, @by, bz)));
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                Trace.WriteLine(ex, "QueryController::Get");
                return this.NotFound();
            }
        }

        //[HttpGet]
        //[Route("sets/{setid}/{version}/query/{detail}/{cx:float}|{cy:float}|{cz:float},{px1:float}|{py1:float}|{pz1:float},{px2:float}|{py2:float}|{pz2:float}")]
        //public IEnumerable<object> Get(string setid, string version, string detail, float cx, float cy, float cz, float px1, float py1, float pz1, float px2, float py2, float pz2)
        //{
        //    return new string[] { "cube1", "cube2" };
        //}
    }
}