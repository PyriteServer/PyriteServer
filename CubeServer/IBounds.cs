// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IBounds.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using Microsoft.Xna.Framework;

    public interface IBounds<TObject>
    {
        BoundingBox BoundingBox { get; }

        BoundingSphere BoundingSphere { get; }

        Intersection<TObject> Intersects(Ray ray);

        Intersection<TObject> Intersects(TObject obj);

        Intersection<TObject> Intersects(BoundingBox intersectionBox);

        Intersection<TObject> Intersects(BoundingFrustum frustum);

        Intersection<TObject> Intersects(BoundingSphere intersectionSphere);
    }
}