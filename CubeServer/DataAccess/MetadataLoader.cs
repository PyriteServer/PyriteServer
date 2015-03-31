// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="MetadataLoader.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class MetadataLoader
    {
        public static OcTree<CubeBounds> Load(Stream metadata)
        {
            return Load(metadata, new OcTree<CubeBounds>());
        }

        public static OcTree<CubeBounds> Load(Stream metadata, OcTree<CubeBounds> ocTree)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            if (ocTree == null)
            {
                throw new ArgumentNullException("ocTree");
            }

            Metadata data;
            using (StreamReader tr = new StreamReader(metadata))
            using (JsonTextReader jr = new JsonTextReader(tr))
            {
                data = new JsonSerializer().Deserialize<Metadata>(jr);
            }

            for (int x = 0; x < data.CubeExists.Length; x++)
            {
                var xData = data.CubeExists[x];

                for (int y = 0; y < xData.Length; y++)
                {
                    var xyData = xData[y];
                    for (int z = 0; z < xyData.Length; z++)
                    {
                        var xyzData = xyData[z];

                        if (xyzData)
                        {
                            var cubeBoundingBox = new BoundingBox { Min = new Vector3(x, y, z), Max = new Vector3(x+1,y+1,z+1) };

                            ocTree.Add(new CubeBounds{BoundingBox = cubeBoundingBox});
                        }
                    }
                }
            }

            return ocTree;
        }
    }
}