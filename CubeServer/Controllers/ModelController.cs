using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CubeServer.Controllers
{
    public class ModelController : ApiController
    {
        // GET: sets/{setid}/{version}/model/{detail}/{modelid}
        public object Get(string setid, string version, string detail, string modelid)
        {
            return "modelBits";
        }
    }
}
