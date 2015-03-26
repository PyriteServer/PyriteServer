// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IBounds.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using Microsoft.Xna.Framework;

    public class CubeBounds : IBounds<CubeBounds>
    {
        public BoundingBox BoundingBox { get; set; }
        public BoundingSphere BoundingSphere { get; set; }

        public Intersection<CubeBounds> Intersects(Ray ray)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                if (this.BoundingBox.Intersects(ray) != null)
                    return new Intersection<CubeBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Intersects(ray) != null)
                    return new Intersection<CubeBounds>(this);
            }

            return null;
        }

        public Intersection<CubeBounds> Intersects(CubeBounds obj)
        {
            Intersection<CubeBounds> ir;

            if (obj.BoundingBox.Min != obj.BoundingBox.Max)
            {
                ir = Intersects(obj.BoundingBox);
            }
            else if (obj.BoundingSphere.Radius != 0f)
            {
                ir = Intersects(obj.BoundingSphere);
            }
            else
                return null;

            if (ir != null)
            {
                ir.Object = this;
                ir.OtherObject = obj;
            }

            return ir;
        }

        public Intersection<CubeBounds> Intersects(BoundingSphere intersectionSphere)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                if (this.BoundingBox.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Contains(intersectionSphere) != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }

            return null;
        }

        public Intersection<CubeBounds> Intersects(BoundingBox intersectionBox)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                ContainmentType ct = this.BoundingBox.Contains(intersectionBox);
                if (ct != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Contains(intersectionBox) != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }

            return null;
        }

        public Intersection<CubeBounds> Intersects(BoundingFrustum frustum)
        {
            if (this.BoundingBox.Max != this.BoundingBox.Min)
            {
                ContainmentType ct = this.BoundingBox.Contains(frustum);
                if (ct != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }
            else if (this.BoundingSphere.Radius != 0f)
            {
                if (this.BoundingSphere.Contains(frustum) != ContainmentType.Disjoint)
                    return new Intersection<CubeBounds>(this);
            }

            return null;
        }

        public override string ToString()
        {
            return String.Format("{0} BoundingBox:{1}", this.GetType().Name, this.BoundingBox);
        }
    }
}