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
            return this.DetailLevels.Select(lod => lod.Query(worldCenter));
        }

        public IEnumerable<int[]> Query(string detail, BoundingBox worldBox)
        {
            SetVersionLevelOfDetail lod = this.DetailLevels.FirstOrDefault(l => l.Name == detail);
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            return lod.Query(worldBox);
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

                yield return detailLevel.Query(worldSphere);
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