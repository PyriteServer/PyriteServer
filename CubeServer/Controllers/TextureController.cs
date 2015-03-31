// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="TextureController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using CubeServer.Contracts;
    using CubeServer.Results;

    public class TextureController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{version}/textures/{detail}/{textureid}")]
        public async Task<IHttpActionResult> Get(string setid, string version, string detail, string textureid)
        {
            try
            {
                StorageStream textureStream = await new HttpCubeStorage().GetTextureStream(setid, version, detail, textureid);
                return new StreamResult(textureStream, this.Request);
            }
            catch (NotFoundException)
            {
                return this.NotFound();
            }
        }
    }
}