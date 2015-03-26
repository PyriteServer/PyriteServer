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

        IntersectionRecord<TObject> Intersects(Ray ray);

        IntersectionRecord<TObject> Intersects(TObject obj);

        IntersectionRecord<TObject> Intersects(BoundingFrustum frustum);
    }
}