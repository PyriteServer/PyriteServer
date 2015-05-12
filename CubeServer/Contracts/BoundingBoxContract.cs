// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="BoundingBoxContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class BoundingBoxContract
    {
        private BoundingBox box;

        public BoundingBoxContract(BoundingBox box)
        {
            this.box = box;
        }

        [JsonProperty("max")]
        public Vector3Contract Max 
        {
            get { return new Vector3Contract(box.Max); } 
        }

        [JsonProperty("min")]
        public Vector3Contract Min
        {
            get
            {
                return new Vector3Contract(box.Min);
            }
        }
    }
}