// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersionLevelOfDetail.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
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
        public int VertexCount { get; set; }

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

        // TODO: Rename this model bounds
        public BoundingBox WorldBounds { get; set; }

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

        public Vector3 WorldToCubeRatio
        {
            get { return this.worldToCubeRatio; }
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