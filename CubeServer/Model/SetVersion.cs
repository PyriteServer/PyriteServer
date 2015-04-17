// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersion.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CubeServer.Contracts;
    using Microsoft.Xna.Framework;

    public class SetVersion
    {
        public SetVersionLevelOfDetail[] DetailLevels { get; set; }
        public DateTime Loaded { get; set; }
        public Uri Material { get; set; }
        public string Name { get; set; }
        public Uri SourceUri { get; set; }
        public string Version { get; set; }

        public IEnumerable<QueryDetailContract> Query(Vector3 worldCenter)
        {
            foreach (SetVersionLevelOfDetail lod in this.DetailLevels)
            {
                Vector3 cubeCenter = lod.ToCubeCoordinates(worldCenter);
                Vector3 flooredCube = new Vector3((int)cubeCenter.X, (int)cubeCenter.Y, (int)cubeCenter.Z);
                BoundingBox rubiksCube = new BoundingBox(flooredCube - Vector3.One, flooredCube + Vector3.One);

                IEnumerable<Intersection<CubeBounds>> queryResults = lod.Cubes.AllIntersections(rubiksCube);

                yield return
                    new QueryDetailContract
                    {
                        Name = lod.Name,
                        Cubes = queryResults.Select(i => i.Object.BoundingBox.Min).Select(v => new[] { (int)v.X, (int)v.Y, (int)v.Z })
                    };
            }
        }

        public IEnumerable<int[]> Query(string detail, BoundingBox worldBox)
        {
            SetVersionLevelOfDetail lod = this.DetailLevels.FirstOrDefault(l => l.Name == detail);
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            BoundingBox cubeBox = lod.ToCubeCoordinates(worldBox);
            IEnumerable<Intersection<CubeBounds>> intersections = lod.Cubes.AllIntersections(cubeBox);

            foreach (Intersection<CubeBounds> intersection in intersections)
            {
                Vector3 min = intersection.Object.BoundingBox.Min;
                yield return new[] { (int)min.X, (int)min.Y, (int)min.Z };
            }
        }

        public IEnumerable<QueryDetailContract> Query(string profile, BoundingSphere worldSphere)
        {
            ProfileLevel[] profiles = ParseProfile(profile).ToArray();

            // TODO: Move this dictionary into SetVersion
            Dictionary<string, SetVersionLevelOfDetail> detailLevels = this.DetailLevels.ToDictionary(
                lod => lod.Name,
                lod => lod,
                StringComparer.OrdinalIgnoreCase);
            int sumProportions = profiles.Sum(p => p.Proportion);

            float radiusProportionRatio = worldSphere.Radius / sumProportions;

            int runningTotal = 0;

            var queries =
                profiles.Select(p => new { p.Level, Radius = runningTotal += p.Proportion })
                    .Select(p => new { p.Level, Radius = p.Radius * radiusProportionRatio });

            foreach (var query in queries)
            {
                SetVersionLevelOfDetail detailLevel;

                if (!detailLevels.TryGetValue(query.Level, out detailLevel))
                {
                    throw new NotFoundException("detail level");
                }

                Vector3 cubeCenter = detailLevel.ToCubeCoordinates(worldSphere.Center);

                // TODO: Spheres in World Space aren't spheres in cube space, so this factor distorts the query if 
                // there is variation in scaling factor for different dimensions e.g. 3,2,1
                float cubeRadius = detailLevel.ToCubeCoordinates(new Vector3(worldSphere.Radius, 0, 0)).X;

                IEnumerable<Intersection<CubeBounds>> queryResults = detailLevel.Cubes.AllIntersections(new BoundingSphere(cubeCenter, cubeRadius));

                yield return
                    new QueryDetailContract
                    {
                        Name = detailLevel.Name,
                        Cubes = queryResults.Select(i => i.Object.BoundingBox.Min).Select(v => new[] { (int)v.X, (int)v.Y, (int)v.Z })
                    };
            }
        }

        private static IEnumerable<ProfileLevel> ParseProfile(string profileString)
        {
            foreach (string level in profileString.Split(ProfileLevel.LevelDelimiter, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] levelSplit = level.Split(ProfileLevel.ProportionDelimiter);
                if (levelSplit.Length != 2)
                {
                    continue;
                }

                int proportion;
                if (!Int32.TryParse(levelSplit[1], out proportion))
                {
                    continue;
                }

                yield return new ProfileLevel { Level = levelSplit[0], Proportion = proportion };
            }
        }

        private struct ProfileLevel
        {
            internal static readonly char[] LevelDelimiter = new char[] { ',' };
            internal static readonly char[] ProportionDelimiter = new char[] { '=' };
            internal string Level;
            internal int Proportion;
        }
    }
}