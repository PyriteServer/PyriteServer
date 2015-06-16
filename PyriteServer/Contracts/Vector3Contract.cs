// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Vector3Contract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Contracts
{
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class Vector3Contract
    {
        private Vector3 vector3;

        public Vector3Contract(Vector3 vector3)
        {
            this.vector3 = vector3;
        }

        [JsonProperty("x")]
        public float X
        {
            get { return this.vector3.X; }
        }

        [JsonProperty("y")]
        public float Y
        {
            get { return this.vector3.Y; }
        }

        [JsonProperty("z")]
        public float Z
        {
            get { return this.vector3.Z; }
        }
    }
}