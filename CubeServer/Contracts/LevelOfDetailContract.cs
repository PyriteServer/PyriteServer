// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="LevelOfDetailContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using Newtonsoft.Json;

    public class LevelOfDetailContract
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("setSize")]
        public Vector3Contract SetSize { get; set; }

        [JsonProperty("textureSetSize")]
        public Vector2Contract TextureSetSize { get; set; }

        [JsonProperty("modelBounds")]
        public BoundingBoxContract ModelBounds { get; set; }

        [JsonProperty("worldBounds")]
        public BoundingBoxContract WorldBounds { get; set; }

        [JsonProperty("worldCubeScale")]
        public Vector3Contract WorldCubeScaling { get; set; }

        [JsonProperty("vertexCount")]
        public int VertexCount { get; set; }
    }
}