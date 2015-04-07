// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UriStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using CubeServer.Contracts;
    using CubeServer.DataAccess.Json;
    using CubeServer.Model;
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;

    public class UriStorage : ICubeStorage, IDisposable
    {
        private const string VERSION_PLACEHOLDER = "{v}";
        private const string X_PLACEHOLDER = "{x}";
        private const string Y_PLACEHOLDER = "{y}";
        private const string Z_PLACEHOLDER = "{z}";
        private bool disposed = false;
        private RevolvingState<LoaderResults> loadedSetData = new RevolvingState<LoaderResults>();
        private Thread loaderThread;
        private ManualResetEvent onExit = new ManualResetEvent(false);
        private AutoResetEvent onLoad = new AutoResetEvent(false);
        private AutoResetEvent onLoadComplete = new AutoResetEvent(false);
        protected string storageRoot;

        public UriStorage()
        {
        }

        public UriStorage(string rootUri)
        {
            this.storageRoot = rootUri;
            this.loaderThread = new Thread(this.LoaderThread);
            this.loaderThread.Start();
        }

        public LoaderResults LastKnownGood
        {
            get { return this.loadedSetData.Get(); }
        }

        public LoaderResults LastLoaderResults { get; set; }

        public WaitHandle WaitLoad
        {
            get { return this.onLoad; }
        }

        public WaitHandle WaitLoadCompleted
        {
            get { return this.onLoadComplete; }
        }

        public SetVersionResultContract GetSetVersion(string setId, string versionId)
        {
            SetVersionResultContract result = new SetVersionResultContract();

            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                throw new NotFoundException("set data");
            }

            Set set;
            if (!setData.Sets.TryGetValue(setId, out set))
            {
                throw new NotFoundException("set");
            }

            var version = set.Versions.FirstOrDefault(v => v.Name.Equals(versionId, StringComparison.InvariantCultureIgnoreCase));
            if (version == null)
            {
                throw new NotFoundException("version");
            }

            result.Set = set.Name;
            result.Version = version.Name;
            result.TextureSize = new Vector2Contract(set.TextureDivisions);
            result.DetailLevels = version.DetailLevels.Select(l => new LevelOfDetailContract { Name = l.Name, SetSize = new Vector3Contract(l.SetSize), WorldBounds = new BoundingBoxContract(l.WorldBounds)}).ToArray();

            return result;
        }

        public IEnumerable<VersionResultContract> EnumerateSetVersions(string setId)
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                return new VersionResultContract[] { };
            }

            Set set;

            if (!setData.Sets.TryGetValue(setId, out set))
            {
                return new VersionResultContract[] { };
            }

            return set.Versions.Select(v => new VersionResultContract { Name = v.Name });
        }

        public IEnumerable<SetResultContract> EnumerateSets()
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                return new SetResultContract[] { };
            }

            return setData.Sets.Values.Select(s => new SetResultContract { Name = s.Name });
        }

        public Task<StorageStream> GetTextureStream(string setId, string version, string detail, string xpos, string ypos)
        {
            if (this.LastKnownGood == null || this.LastKnownGood.Sets == null || this.LastKnownGood.Sets[setId] == null ||
                this.LastKnownGood.Sets[setId].TexturePathFormat == null)
            {
                throw new NotFoundException("Texture");
            }
            string texturePath = this.LastKnownGood.Sets[setId].TexturePathFormat;
            texturePath = texturePath.Replace(VERSION_PLACEHOLDER, version);
            texturePath = texturePath.Replace(X_PLACEHOLDER, xpos);
            texturePath = texturePath.Replace(Y_PLACEHOLDER, ypos);
            return GetStorageStreamForPath(texturePath);
        }

        public Task<StorageStream> GetModelStream(string setId, string version, int detail, string xpos, string ypos, string zpos)
        {
            if (this.LastKnownGood == null || this.LastKnownGood.Sets == null || this.LastKnownGood.Sets[setId] == null ||
                this.LastKnownGood.Sets[setId].Versions == null)
            {
                throw new NotFoundException("model");
            }

            SetVersion setVersion = this.LastKnownGood.Sets[setId].Versions.First((sv) => sv.Name == version);
            if (setVersion == null)
            {
                throw new NotFoundException("version");
            }

            SetVersionLevelOfDetail lod = setVersion.DetailLevels.First(l => l.Number == detail);
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            string modelPath = lod.CubePathFormat;

            modelPath = modelPath.Replace(X_PLACEHOLDER, xpos);
            modelPath = modelPath.Replace(Y_PLACEHOLDER, ypos);
            modelPath = modelPath.Replace(Z_PLACEHOLDER, zpos);

            return GetStorageStreamForPath(modelPath);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<T> Deserialize<T>(Uri url)
        {
            Func<Stream, T> deserialize = sourceStream =>
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
            url = this.TransformUri(url);

            Trace.WriteLine(url.ToString(), "UriStorage::Get");

            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            {
                return perform(stream);
            }
        }

        private async Task<StorageStream> GetStorageStreamForPath(string path)
        {
            Uri targetUri = new Uri(path);
            targetUri = this.TransformUri(targetUri);
            WebRequest request = WebRequest.Create(targetUri);
            WebResponse response = await request.GetResponseAsync();

            // Storage stream is used in a StreamResult which closes the stream for us when done
            return new StorageStream(response.GetResponseStream(), response.ContentLength, new MediaTypeHeaderValue(response.ContentType));
        }

        public async Task<LoaderResults> LoadMetadata()
        {
            LoaderResults results = new LoaderResults();

            List<LoaderException> exceptions = new List<LoaderException>();
            Dictionary<string, Set> sets = new Dictionary<string, Set>(StringComparer.InvariantCultureIgnoreCase);

            SetContract[] setsMetadata = null;
            Uri storageRootUri = null;
            try
            {
                storageRootUri = new Uri(this.storageRoot);

                setsMetadata = await this.Deserialize<SetContract[]>(storageRootUri);
                if (setsMetadata == null)
                {
                    throw new SerializationException("Deserialization Failed");
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(new LoaderException("Sets", this.storageRoot, ex));
                results.Errors = exceptions.ToArray();
                results.Sets = sets;
                return results;
            }

            foreach (SetContract set in setsMetadata)
            {
                try
                {
                    Uri setMetadataUri = new Uri(storageRootUri, set.Url);
                    Trace.WriteLine(String.Format("Set: {0}, Url {1}", set.Name, setMetadataUri));
                    SetMetadataContract setMetadata = await this.Deserialize<SetMetadataContract>(setMetadataUri);
                    if (setMetadata == null)
                    {
                        throw new SerializationException("Set metadata deserialization Failed");
                    }

                    Trace.WriteLine(String.Format("Discovered set {0} at {1}", set.Name, set.Url));

                    Set currentSet = new Set
                                     {
                                         SourceUri = setMetadataUri,
                                         Name = set.Name,
                                         TextureDivisions = new Vector2(setMetadata.TextureSubdivide, setMetadata.TextureSubdivide),
                                         TexturePathFormat = setMetadata.TexturePath
                                     };

                    // TODO: Set versioning story

                    List<SetVersionLevelOfDetail> detailLevels = new List<SetVersionLevelOfDetail>();
                    foreach (int detailLevel in Enumerable.Range(setMetadata.MinimumViewport, setMetadata.MaximumViewport))
                    {
                        string detailLevelMetadata = setMetadata.MetadataTemplate.Replace("{v}", detailLevel.ToString(CultureInfo.InvariantCulture));
                        Uri detailLevelMetadataUri = new Uri(setMetadataUri, detailLevelMetadata);

                        CubeMetadataContract cubeMetadata = await this.Deserialize<CubeMetadataContract>(detailLevelMetadataUri);

                        OcTree<CubeBounds> octree = MetadataLoader.Load(cubeMetadata);
                        octree.UpdateTree();

                        var cubeBounds = cubeMetadata.GridSize;
                        var worldBounds = cubeMetadata.Extents;

                        SetVersionLevelOfDetail currentSetLevelOfDetail = new SetVersionLevelOfDetail
                                                       {
                                                           // TODO replace this with l<detailIndex>
                                                           Name = "v" + detailLevel.ToString(CultureInfo.InvariantCulture),
                                                           SetSize = new Vector3(cubeBounds.X, cubeBounds.Y, cubeBounds.Z),
                                                           WorldBounds = new BoundingBox(new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin), new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax)),
                                                           Cubes = octree,
                                                           Number = detailLevel,
                                                           CubePathFormat =
                                                               setMetadata.CubeTemplate.Replace(
                                                                   "{v}",
                                                                   detailLevel.ToString(CultureInfo.InvariantCulture)).Replace(".obj", ".ebo")
                                                       };

                        detailLevels.Add(currentSetLevelOfDetail);
                    }

                    // TODO Set revisioning and versioning story
                    currentSet.Versions = new []{new SetVersion { Name = "version1" , DetailLevels = detailLevels.OrderBy(v => v.Number).ToArray()}};

                    sets.Add(currentSet.Name, currentSet);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new LoaderException("Set", set.Url, ex));
                }
            }

            results.Errors = exceptions.ToArray();
            results.Sets = sets;

            return results;
        }

        protected virtual Uri TransformUri(Uri sourceUri)
        {
            return sourceUri;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || this.disposed)
            {
                return;
            }

            if (this.loaderThread != null)
            {
                this.onExit.Set();
                this.loaderThread.Join(10000);
                this.loaderThread.Abort();
                this.loaderThread = null;
            }

            if (this.onExit != null)
            {
                this.onExit.Dispose();
                this.onExit = null;
            }

            if (this.onLoadComplete != null)
            {
                this.onLoadComplete.Dispose();
                this.onLoadComplete = null;
            }

            if (this.onLoad != null)
            {
                this.onLoad.Dispose();
                this.onLoad = null;
            }

            if (this.loadedSetData != null)
            {
                this.loadedSetData.Dispose();
                this.loadedSetData = null;
            }

            this.disposed = true;
        }

        private void LoaderThread()
        {
            TimeSpan pollingPeriod = TimeSpan.FromMilliseconds(500);
            TimeSpan reload = TimeSpan.FromMinutes(30);
            DateTime next = DateTime.MinValue;

            while (!this.onExit.WaitOne(0))
            {
                if (DateTime.Now > next)
                {
                    LoaderResults results = this.LoadMetadata().Result;
                    if (results.Errors.Length == 0)
                    {
                        this.loadedSetData.Set(results);
                        this.onLoad.Set();
                    }
                    else
                    {
                        this.LastLoaderResults = results;
                    }

                    this.onLoadComplete.Set();
                    next = DateTime.Now + reload;
                }
                else
                {
                    Thread.Sleep(pollingPeriod);
                }
            }
        }

        private class RevolvingState<T> : IDisposable
        {
            private readonly ReaderWriterLockSlim stateLock = new ReaderWriterLockSlim();
            private readonly T[] states = new T[2];
            private volatile int active = -1;
            private bool disposed = false;

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            public T Get()
            {
                try
                {
                    this.stateLock.EnterReadLock();
                    if (this.active != -1)
                    {
                        return this.states[this.active];
                    }
                    return default(T);
                }
                finally
                {
                    this.stateLock.ExitReadLock();
                }
            }

            public void Set(T value)
            {
                switch (this.active)
                {
                    case -1:
                    {
                        this.Set(0, value);
                        break;
                    }
                    case 0:
                    {
                        this.Set(1, value);
                        break;
                    }
                    case 1:
                    {
                        this.Set(0, value);
                        break;
                    }
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposing || this.disposed)
                {
                    return;
                }

                if (this.stateLock != null)
                {
                    this.stateLock.Dispose();
                }

                this.disposed = true;
            }

            private void Set(int index, T value)
            {
                this.states[index] = value;
                try
                {
                    this.stateLock.EnterWriteLock();
                    this.active = index;
                }
                finally
                {
                    this.stateLock.ExitWriteLock();
                }
            }
        }
    }
}