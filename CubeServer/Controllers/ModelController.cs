// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ModelController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Web.Http;

    public class ModelController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{version}/models/{detailLevel}/{modelid}")]
        public object Get(string setid, string version, string detailLevel, string modelid)
        {
            return "modelBits";
        }
    }
}