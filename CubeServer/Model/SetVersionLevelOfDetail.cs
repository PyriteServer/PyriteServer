// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersionLevelOfDetail.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CubeServer.Contracts;
    using Microsoft.Xna.Framework;

    public class SetVersionLevelOfDetail
    {
        private Vector3 setSize = Vector3.One;
        private BoundingBox virtualWorldBounds = new BoundingBox();
        private Vector3 worldToCubeRatio = Vector3.One;

        public OcTree<CubeBounds> Cubes { get; set; }
        public Uri Metadata { get; set; }
        public Uri ModelTemplate { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }

        public Vector3 SetSize
        {
            get { return this.setSize; }
            set
            {
                this.setSize = value;
                this.UpdateScale();
            }
        }

        public Vector2 TextureSetSize { get; set; }
        public Uri TextureTemplate { get; set; }
        public int VertexCount { get; set; }

        // TODO: Rename this model bounds

        // TODO: Rename this to WorldBounds, and fix up all callers
        public BoundingBox VirtualWorldBounds
        {
            get { return this.virtualWorldBounds; }
            set
            {
                this.virtualWorldBounds = value;
                this.UpdateScale();
            }
        }

        public BoundingBox WorldBounds { get; set; }

        public Vector3 WorldToCubeRatio
        {
            get { return this.worldToCubeRatio; }
        }

        public QueryDetailContract Query(Vector3 worldCenter)
        {
            Vector3 cubeCenter = this.ToCubeCoordinates(worldCenter);
            Vector3 flooredCube = new Vector3((int)cubeCenter.X, (int)cubeCenter.Y, (int)cubeCenter.Z);

            // all vertices and edges of the "inner" cube touch one or more points on all outer cubes.
            BoundingBox rubiksCube = new BoundingBox(flooredCube, flooredCube + Vector3.One);

            IEnumerable<Intersection<CubeBounds>> queryResults = this.Cubes.AllIntersections(rubiksCube);

            return new QueryDetailContract
                   {
                       Name = this.Name,
                       Cubes = queryResults.Select(i => i.Object.BoundingBox.Min).Select(v => new[] { (int)v.X, (int)v.Y, (int)v.Z })
                   };
        }

        public QueryDetailContract Query(BoundingSphere worldSphere)
        {
            Vector3 cubeCenter = this.ToCubeCoordinates(worldSphere.Center);

            // TODO: Spheres in World Space aren't spheres in cube space, so this factor distorts the query if 
            // there is variation in scaling factor for different dimensions e.g. 3,2,1
            float cubeRadius = this.ToCubeCoordinates(new Vector3(worldSphere.Radius, 0, 0)).X;

            IEnumerable<Intersection<CubeBounds>> queryResults = this.Cubes.AllIntersections(new BoundingSphere(cubeCenter, cubeRadius));

            return new QueryDetailContract
                   {
                       Name = this.Name,
                       Cubes = queryResults.Select(i => i.Object.BoundingBox.Min).Select(v => new[] { (int)v.X, (int)v.Y, (int)v.Z })
                   };
        }

        public IEnumerable<int[]> Query(BoundingBox worldBox)
        {
            BoundingBox cubeBox = this.ToCubeCoordinates(worldBox);
            IEnumerable<Intersection<CubeBounds>> intersections = this.Cubes.AllIntersections(cubeBox);

            foreach (Intersection<CubeBounds> intersection in intersections)
            {
                Vector3 min = intersection.Object.BoundingBox.Min;
                yield return new[] { (int)min.X, (int)min.Y, (int)min.Z };
            }
        }

        public Vector3 ToCubeCoordinates(Vector3 worldCoordinates)
        {
            Vector3 zeroBaseWorld = worldCoordinates - this.virtualWorldBounds.Min;
            return zeroBaseWorld / this.worldToCubeRatio;
        }

        public BoundingBox ToCubeCoordinates(BoundingBox worldCoordinates)
        {
            Vector3 min = worldCoordinates.Min - this.virtualWorldBounds.Min;
            min /= this.worldToCubeRatio;

            Vector3 max = worldCoordinates.Max - this.virtualWorldBounds.Min;
            max /= this.worldToCubeRatio;

            return new BoundingBox(min, max);
        }

        public Vector3 ToWorldCoordinates(Vector3 cubeCoordinates)
        {
            Vector3 scaleCubeToWorld = this.worldToCubeRatio * cubeCoordinates;
            return scaleCubeToWorld + this.virtualWorldBounds.Min;
        }

        public BoundingBox ToWorldCoordinates(BoundingBox cubeCoordinates)
        {
            Vector3 min = this.worldToCubeRatio * cubeCoordinates.Min;
            min += this.virtualWorldBounds.Min;

            Vector3 max = this.worldToCubeRatio * cubeCoordinates.Max;
            max += this.virtualWorldBounds.Max;

            return new BoundingBox(min, max);
        }

        private void UpdateScale()
        {
            if (this.setSize == Vector3.Zero)
            {
                this.worldToCubeRatio = Vector3.One;
                return;
            }

            BoundingBox value = this.virtualWorldBounds;
            this.worldToCubeRatio = (value.Max - value.Min) / this.SetSize;
        }
    }
}