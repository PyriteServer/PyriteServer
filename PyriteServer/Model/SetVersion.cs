// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SetVersion.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using PyriteServer.Contracts;

    public class SetVersion
    {
        public SortedDictionary<string, SetVersionLevelOfDetail> DetailLevels { get; set; }
        public DateTime Loaded { get; set; }
        public Uri Material { get; set; }
        public string Name { get; set; }
        public Uri SourceUri { get; set; }
        public string Version { get; set; }

        private Lazy<SetVersionLevelOfDetail> reference;

        public SetVersion()
        {
            this.reference = new Lazy<SetVersionLevelOfDetail>(this.ReferenceLevelOfDetail, true);
        }

        private SetVersionLevelOfDetail ReferenceLevelOfDetail()
        {
            if (this.DetailLevels.Count > 1)
            {
                return this.DetailLevels.Values.Skip(1).FirstOrDefault();
            }
            return this.DetailLevels.Values.FirstOrDefault();
        }

        public IEnumerable<QueryDetailContract> Query(string boundaryReferenceName, Vector3 worldCenter)
        {
            // reframe center in context of world coordinates for the center of the nearest reference container.

            SetVersionLevelOfDetail boundaryReference;
            if (!this.DetailLevels.TryGetValue(boundaryReferenceName, out boundaryReference))
            {
                throw new NotFoundException("boundaryReference");
            }

            Vector3 referenceCube = boundaryReference.ToCubeCoordinates(worldCenter);
            Vector3 referenceCubeCenter = new Vector3((int)referenceCube.X, (int)referenceCube.Y, (int)referenceCube.Z) + new Vector3(0.5f,0.5f,0.5f) ;
            Vector3 adjustedCenter = boundaryReference.ToWorldCoordinates(referenceCubeCenter);

            return this.DetailLevels.Values.Select(lod => lod.Query(adjustedCenter));
        }

        public IEnumerable<int[]> Query(string detail, BoundingBox worldBox)
        {
            SetVersionLevelOfDetail lod = this.DetailLevels.Values.FirstOrDefault(l => l.Name == detail);
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            return lod.Query(worldBox);
        }

        public IEnumerable<QueryDetailContract> Query(string profile, BoundingSphere worldSphere)
        {
            ProfileLevel[] profiles = ParseProfile(profile).ToArray();
            int sumProportions = profiles.Sum(p => p.Proportion);

            float radiusProportionRatio = worldSphere.Radius / sumProportions;


            int runningTotal = 0;

            var queries =
                profiles.Select(p => new { p.Level, Radius = runningTotal += p.Proportion })
                    .Select(p => new { p.Level, Radius = p.Radius * radiusProportionRatio });

            foreach (var query in queries)
            {
                SetVersionLevelOfDetail detailLevel;

                if (!this.DetailLevels.TryGetValue(query.Level, out detailLevel))
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