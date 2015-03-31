// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UriStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using CubeServer.Contracts;
    using CubeServer.DataAccess;
    using Newtonsoft.Json;

    public class UriStorage : ICubeStorage
    {
        private readonly string storageRoot;

        public UriStorage(string rootUri)
        {
            this.storageRoot = rootUri;
        }

        public IEnumerable<VersionResultContract> EnumerateSetVersions(string setId)
        {
            throw new NotImplementedException();
        }

        public Task<StorageStream> GetTextureStream(string setId, string version, string detail, string textureid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SetResultContract> EnumerateSets()
        {
            throw new NotImplementedException();
        }

        public async Task<T> Deserialize<T>(Uri url)
        {
            Func<Stream,T> deserialize = sourceStream =>
            {
                using (StreamReader sr = new StreamReader(sourceStream))
                using (JsonTextReader jr = new JsonTextReader(sr))
                {
                    return new JsonSerializer().Deserialize<T>(jr);
                }
            };

            return await this.Get(url, deserialize);
        }

        public async Task<T> Get<T>(Uri url, Func<Stream, T> perform)
        {
            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            {
                return perform(stream);
            } 
        }

        public async Task<LoaderResults> Load()
        {
            LoaderResults results = new LoaderResults();

            List<LoaderException> exceptions = new List<LoaderException>();
            List<Set> sets = new List<Set>();

            SetContract[] setsMetadata = null;
            Uri storageRootUri = null;
            try
            {
                storageRootUri = new Uri(storageRoot);
                setsMetadata = await this.Deserialize<SetContract[]>(storageRootUri);
                if (setsMetadata == null)
                {
                    throw new SerializationException("Deserialization Failed");
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(new LoaderException("Sets",this.storageRoot, ex));
                results.Errors = exceptions.ToArray();
                results.Sets = new Set[] { };
                results.Success = false;
            }

            foreach (SetContract set in setsMetadata)
            {
                try
                {
                    Uri setMetadataUri = new Uri(storageRootUri, set.Url);
                    Trace.WriteLine(string.Format("Set: {0}, Url {1}", set.Name, setMetadataUri));
                    SetMetadataContract setMetadata = await this.Deserialize<SetMetadataContract>(setMetadataUri);
                    if (setMetadata == null)
                    {
                        throw new SerializationException("Deserialization Failed");
                    }

                    Trace.WriteLine(String.Format("Discovered set {0} at {1}", set.Name, set.Url));

                    Set currentSet = new Set
                                     {
                                         SourceUri = setMetadataUri, 
                                         Name = set.Name
                                     };
                    

                    List<SetVersion> versions = new List<SetVersion>();
                    foreach (int version in Enumerable.Range(setMetadata.MinimumViewport, setMetadata.MaximumViewport))
                    {
                        string versionMetadata = setMetadata.MetadataTemplate.Replace("{v}", version.ToString(CultureInfo.InvariantCulture));
                        Uri versionMetadataUri = new Uri(setMetadataUri, versionMetadata);

                        var octree = await this.Get(versionMetadataUri, MetadataLoader.Load);
                        octree.UpdateTree();

                        SetVersion currentSetVersion = new SetVersion { Cubes = octree, Number = version };

                        versions.Add(currentSetVersion);
                    }
                    currentSet.Versions = versions.OrderBy(v => v.Number).ToArray();
                    sets.Add(currentSet);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new LoaderException("Set", set.Url, ex));
                }
            }

            results.Sets = sets.OrderBy(set => set.Name).ToArray();
            results.Errors = exceptions.ToArray();

            return results;
        }

        public class SetContract
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class SetMetadataContract
        {
            public int MinimumViewport { get; set; }
            public int MaximumViewport { get; set; }
            public string CubeTemplate { get; set; }
            public string MtlTemplate { get; set; }
            public string JpgTemplate { get; set; }
            public string MetadataTemplate { get; set; }
            public int TextureSubdivide { get; set; }
            public string TexturePath { get; set; }            
        }

        public class SetVersionContract
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }

    public class LoaderException : Exception
    {
        public LoaderException(string stage, string uri, Exception innerException) : base(innerException.Message, innerException)
        {
            this.Stage = stage;
            this.Uri = uri;
        }

        public string Stage { get; set; }
        public string Uri { get; set; }
    }

    public class SetVersion
    {
        public int Number { get; set; }
        public OcTree<CubeBounds> Cubes { get; set; } 
    }
}