// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="CacheHeaderActionFilter.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;

    public class CacheControlAttribute : ActionFilterAttribute
    {
        private readonly uint maxAge;

        public CacheControlAttribute(uint maxAge)
        {
            this.maxAge = maxAge;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null || actionExecutedContext.Response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(maxAge), Public = true };
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}