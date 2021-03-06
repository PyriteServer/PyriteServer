﻿// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryController.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Http;
    using Microsoft.Xna.Framework;
    using PyriteServer.Contracts;

    public class QueryController : ApiController
    {
        [HttpGet]
        [Route("sets/{setid}/{versionId}/query/{detail}/{ax},{ay},{az}/{bx},{by},{bz}")]
        public IHttpActionResult BoundingBoxQuery(string setId, string versionId, string detail, float ax, float ay, float az, float bx, float by, float bz)
        {
            try
            {
                IEnumerable<int[]> result = Dependency.Storage.Query(
                    setId,
                    versionId,
                    detail,
                    new BoundingBox(new Vector3(ax, ay, az), new Vector3(bx, @by, bz)));
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                Trace.WriteLine(ex, "QueryController::Get");
                return this.NotFound();
            }
        }

        [HttpGet]
        [Route("sets/{setid}/{versionId}/query/{profile}/{ax},{ay},{az}/{radius}")]
        public IHttpActionResult BoundingBoxSphere(string setId, string versionId, string profile, float ax, float ay, float az, float radius)
        {
            try
            {
                IEnumerable<QueryDetailContract> result = Dependency.Storage.Query(
                    setId,
                    versionId,
                    profile,
                    new BoundingSphere(new Vector3(ax, ay, az), radius));
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                Trace.WriteLine(ex, "QueryController::Get");
                return this.NotFound();
            }
        }

        [HttpGet]
        [Route("sets/{setid}/{versionId}/query/3x3/{reference}/{ax},{ay},{az}")]
        public IHttpActionResult BoundingBoxSphere(string setId, string versionId, string reference, float ax, float ay, float az)
        {
            try
            {
                IEnumerable<QueryDetailContract> result = Dependency.Storage.Query(
                    setId,
                    versionId,
                    reference,
                    new Vector3(ax, ay, az));
                return this.Ok(ResultWrapper.OkResult(result));
            }
            catch (NotFoundException ex)
            {
                Trace.WriteLine(ex, "QueryController::Get");
                return this.NotFound();
            }
        }
    }
}