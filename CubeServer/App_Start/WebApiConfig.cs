// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="WebApiConfig.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System.Web.Http;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "GetAllSets", 
                routeTemplate: "sets/", 
                defaults: new { controller = "Set", action = "GetAll" });

            config.Routes.MapHttpRoute(
                name: "GetSetMetadata", 
                routeTemplate: "sets/{setid}", 
                defaults: new { controller = "Set", action = "Get" });

            config.Routes.MapHttpRoute(
                name: "GetSetVersion",
                routeTemplate: "sets/{setid}/{versionid}",
                defaults: new { controller = "Version", action = "Get" });

            config.Routes.MapHttpRoute(
                name: "GetSetVersionDetailCube",
                routeTemplate: "sets/{setid}/{version}/cubes/{detail}/{x},{y},{z},{h},{w},{d}",
                defaults: new { controller = "Cube", action = "Get" });

            config.Routes.MapHttpRoute(
                name: "GetSetVersionDetailTexture",
                routeTemplate: "sets/{setid}/{version}/textures/{detail}/{textureid}",
                defaults: new { controller = "Texture", action = "Get" });

            config.Routes.MapHttpRoute(
                name: "GetSetVersionDetailModel",
                routeTemplate: "sets/{setid}/{version}/models/{detail}/{modelid}",
                defaults: new { controller = "Model", action = "Get" });
        }
    }
}