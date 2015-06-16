// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="MetadataLoader.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;
    using PyriteServer.DataAccess.Json;
    using PyriteServer.Model;

    public class MetadataLoader
    {
        public static OcTree<CubeBounds> Load(Stream metadata, string name, Vector3 cubeSize)
        {
            return Load(metadata, new OcTree<CubeBounds>(), name, cubeSize);
        }

        public static OcTree<CubeBounds> Load(CubeMetadataContract data, string name, Vector3 cubeSize)
        {
            return Load(data, new OcTree<CubeBounds>(), name, cubeSize);
        }

        public static OcTree<CubeBounds> Load(Stream metadata, OcTree<CubeBounds> ocTree, string name, Vector3 cubeSize)
        {
            CubeMetadataContract data;
            using (StreamReader tr = new StreamReader(metadata))
            using (JsonTextReader jr = new JsonTextReader(tr))
            {
                data = new JsonSerializer().Deserialize<CubeMetadataContract>(jr);
            }

            return Load(data, ocTree, name, cubeSize);
        }

        public static OcTree<CubeBounds> Load(CubeMetadataContract data, OcTree<CubeBounds> ocTree, string name, Vector3 cubeSize)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (ocTree == null)
            {
                throw new ArgumentNullException("ocTree");
            }

            ocTree.Add(LoadCubeBounds(data, name, cubeSize));

            return ocTree;
        }

        public static IEnumerable<CubeBounds> LoadCubeBounds(CubeMetadataContract data, string name, Vector3 cubeSize)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            for (int x = 0; x < data.CubeExists.Length; x++)
            {
                bool[][] xData = data.CubeExists[x];

                for (int y = 0; y < xData.Length; y++)
                {
                    bool[] xyData = xData[y];
                    for (int z = 0; z < xyData.Length; z++)
                    {
                        bool xyzData = xyData[z];

                        if (xyzData)
                        {
                            BoundingBox cubeBoundingBox = new BoundingBox { Min = new Vector3(x, y, z), Max = new Vector3(x + cubeSize.X, y + cubeSize.Y, z + cubeSize.Z) };

                            yield return new CubeBounds { BoundingBox = cubeBoundingBox, LevelOfDetail = name};
                        }
                    }
                }
            }
        }
    }
}