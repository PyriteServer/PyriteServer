// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="TextureController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Web.Http;

    public class TextureController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{version}/textures/{detail}/{textureid}")]
        public object Get(string setid, string version, string detail, string textureid)
        {
            return "textureBits";
        }
    }
}