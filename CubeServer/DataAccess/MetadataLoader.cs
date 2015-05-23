// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="MetadataLoader.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CubeServer.DataAccess.Json;
    using CubeServer.Model;
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class MetadataLoader
    {
        public static OcTree<CubeBounds> Load(Stream metadata)
        {
            return Load(metadata, new OcTree<CubeBounds>());
        }

        public static OcTree<CubeBounds> Load(CubeMetadataContract data)
        {
            return Load(data, new OcTree<CubeBounds>());
        }

        public static OcTree<CubeBounds> Load(Stream metadata, OcTree<CubeBounds> ocTree)
        {
            CubeMetadataContract data;
            using (StreamReader tr = new StreamReader(metadata))
            using (JsonTextReader jr = new JsonTextReader(tr))
            {
                data = new JsonSerializer().Deserialize<CubeMetadataContract>(jr);
            }

            return Load(data, ocTree);
        }

        public static OcTree<CubeBounds> Load(CubeMetadataContract data, OcTree<CubeBounds> ocTree)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (ocTree == null)
            {
                throw new ArgumentNullException("ocTree");
            }

            ocTree.Add(LoadCubeBounds(data));

            return ocTree;
        }

        public static IEnumerable<CubeBounds> LoadCubeBounds(CubeMetadataContract data)
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
                            // TODO: tranform cubebounding box into universal space
                            BoundingBox cubeBoundingBox = new BoundingBox { Min = new Vector3(x, y, z), Max = new Vector3(x + 1, y + 1, z + 1) };

                            yield return new CubeBounds { BoundingBox = cubeBoundingBox };
                        }
                    }
                }
            }
        }
    }
}