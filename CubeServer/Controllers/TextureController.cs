using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CubeServer.Controllers
{
    public class TextureController : ApiController
    {
        // GET: sets/{setid}/{version}/textures/{detail}/{textureid}
        public object Get(string setid, string version, string detail, string textureid)
        {
            return "textureBits";
        }
    }
}
