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
        // GET: sets/{setid}/{version}/cubes/{detail}/{x},{y},{z},{h},{w},{d}
        public IEnumerable<object> Get(string setid, string version, string detail, float x, float y, float z, float h, float w, float d)
        {
            return new string[] { "cube1", "cube2" };
        }
    }
}