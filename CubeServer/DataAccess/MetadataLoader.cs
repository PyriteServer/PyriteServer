// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="MetadataLoader.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class MetadataLoader
    {
        public OctTree<CubeBounds> Load(Stream metadata)
        {
            Metadata data;
            using (StreamReader tr = new StreamReader(metadata))
            using (JsonTextReader jr = new JsonTextReader(tr))
            {
                data = new JsonSerializer().Deserialize<Metadata>(jr);
            }

            OctTree<CubeBounds> octTree = new OctTree<CubeBounds>();

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

                            octTree.Add(new CubeBounds{BoundingBox = cubeBoundingBox});
                        }
                    }
                }
            }

            return octTree;
        }
    }
}