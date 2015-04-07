// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Vector2Contract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class Vector2Contract
    {
        private Vector2 vector2;

        public Vector2Contract(Vector2 vector2)
        {
            this.vector2 = vector2;
        }

        [JsonProperty("x")]
        public float X
        {
            get { return this.vector2.X; }
        }

        [JsonProperty("y")]
        public float Y
        {
            get { return this.vector2.Y; }
        }
    }
}