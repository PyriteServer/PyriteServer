// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    public class SetController : ApiController
    {

        [HttpGet]
        [Route("sets")]
        public IEnumerable<object> GetAll()
        {
            return new [] { "Nashville", "LasVegas", "Seattle" };
        }

        [HttpGet]
        [Route("sets/{setid}")]
        public IEnumerable<object> Get(string setid)
        {
            return new [] {"v1"};
        }
    }
}