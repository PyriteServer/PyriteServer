// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="CubeController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    public class CubeController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{version}/cubes/{detail}/{x:float},{y:float},{z:float},{h:float},{w:float},{d:float}")]
        public IEnumerable<object> Get(string setid, string version, string detail, float x, float y, float z, float h, float w, float d)
        {
            return new string[] { "cube1", "cube2" };
        }

        [HttpGet]
        [Route("sets/{setid}/{version}/cubes/{detail}/{cx:float}|{cy:float}|{cz:float},{px1:float}|{py1:float}|{pz1:float},{px2:float}|{py2:float}|{pz2:float}")]
        public IEnumerable<object> Get(string setid, string version, string detail, float cx, float cy, float cz, float px1, float py1, float pz1, float px2, float py2, float pz2)
        {
            return new string[] { "cube1", "cube2" };
        }

    }
}