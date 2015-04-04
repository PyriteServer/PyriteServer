// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ModelController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using CubeServer.Contracts;
    using CubeServer.Results;

    public class ModelController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{version}/models/{detailLevel:int}/{xpos},{ypos},{zpos}")]
        public async Task<IHttpActionResult> Get(string setid, string version, int detailLevel, string xpos, string ypos, string zpos)
        {
            try
            {
                StorageStream modelStream = await Dependency.Storage.GetModelStream(setid, version, detailLevel, xpos, ypos, zpos);
                return new StreamResult(modelStream, this.Request);
            }
            catch (NotFoundException)
            {
                return this.NotFound();
            }
        }
    }
}