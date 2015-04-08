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
        protected string storageRoot;
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

        public async Task<T> Deserialize<T>(Uri url)
        {
            return await this.Get(url, this.DeserializeStream<T>);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<VersionResultContract> EnumerateSetVersions(string setId)
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                throw new NotFoundException("setData");
            }

            Dictionary<string, SetVersion> setVersions;
            if (!setData.Sets.TryGetValue(setId, out setVersions))
            {
                return new VersionResultContract[] { };
            }

            return setVersions.Values.Select(v => new VersionResultContract { Name = v.Version });
        }

        public IEnumerable<SetResultContract> EnumerateSets()
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                return new SetResultContract[] { };
            }

            return setData.Sets.Keys.Select(s => new SetResultContract { Name = s });
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

        public Task<StorageStream> GetModelStream(string setId, string version, string detail, string xpos, string ypos, string zpos)
        {
            SetVersion setVersion = this.loadedSetData.Get().FindSetVersion(setId, version);

            SetVersionLevelOfDetail lod = setVersion.DetailLevels.FirstOrDefault(l => l.Name == detail);
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            string modelPath = lod.ModelTemplate.ToString();
            modelPath = modelPath.Replace(X_PLACEHOLDER, xpos);
            modelPath = modelPath.Replace(Y_PLACEHOLDER, ypos);
            modelPath = modelPath.Replace(Z_PLACEHOLDER, zpos);

            return this.GetStorageStreamForPath(modelPath);
        }

        public SetVersionResultContract GetSetVersion(string setId, string versionId)
        {
            SetVersionResultContract result = new SetVersionResultContract();

            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                throw new NotFoundException("setData");
            }

            SetVersion setVersion = setData.FindSetVersion(setId, versionId);

            result.Set = setVersion.Name;
            result.Version = setVersion.Version;
            result.DetailLevels =
                setVersion.DetailLevels.Select(
                    l =>
                        new LevelOfDetailContract
                        {
                            Name = l.Name,
                            SetSize = new Vector3Contract(l.SetSize),
                            WorldBounds = new BoundingBoxContract(l.WorldBounds),
                            TextureSetSize = new Vector2Contract(l.TextureSetSize)
                        }).ToArray();

            return result;
        }

        public Task<StorageStream> GetTextureStream(string setId, string version, string detail, string xpos, string ypos)
        {
            SetVersion setVersion = this.loadedSetData.Get().FindSetVersion(setId, version);

            SetVersionLevelOfDetail lod = setVersion.DetailLevels.FirstOrDefault(l => l.Name.Equals(detail, StringComparison.OrdinalIgnoreCase));
            if (lod == null)
            {
                throw new NotFoundException("detailLevel");
            }

            string texturePath = lod.TextureTemplate.ToString();
            texturePath = texturePath.Replace(X_PLACEHOLDER, xpos);
            texturePath = texturePath.Replace(Y_PLACEHOLDER, ypos);
            return this.GetStorageStreamForPath(texturePath);
        }

        public async Task<LoaderResults> LoadMetadata()
        {
            LoaderResults results = new LoaderResults();

            List<LoaderException> exceptions = new List<LoaderException>();
            Dictionary<string, Dictionary<string, SetVersion>> sets =
                new Dictionary<string, Dictionary<string, SetVersion>>(StringComparer.InvariantCultureIgnoreCase);

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

            List<SetVersion> setVersions = new List<SetVersion>();

            foreach (SetContract set in setsMetadata)
            {
                try
                {
                    foreach (SetVersionContract version in set.Versions)
                    {
                        Uri setMetadataUri = new Uri(storageRootUri, version.Url);

                        Trace.WriteLine(String.Format("Set: {0}, Url {1}", set.Name, setMetadataUri));
                        SetMetadataContract setMetadata = await this.Deserialize<SetMetadataContract>(setMetadataUri);
                        if (setMetadata == null)
                        {
                            throw new SerializationException("Set metadata deserialization Failed");
                        }

                        Trace.WriteLine(String.Format("Discovered set {0}/{1} at {2}", set.Name, version.Name, version.Url));

                        Uri material = new Uri(setMetadataUri, setMetadata.Mtl);

                        SetVersion currentSet = new SetVersion
                                                {
                                                    SourceUri = setMetadataUri,
                                                    Name = set.Name,
                                                    Version = version.Name,
                                                    Material = material
                                                };

                        List<SetVersionLevelOfDetail> detailLevels;
                            
                        detailLevels = await this.ExtractDetailLevels(setMetadata, setMetadataUri);

                        currentSet.DetailLevels = detailLevels.ToArray();
                        setVersions.Add(currentSet);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(new LoaderException("Set", storageRootUri.ToString(), ex));
                }
            }

            sets = setVersions.GroupBy(s => s.Name).ToDictionary(s => s.Key, this.GenerateVersionMap, StringComparer.OrdinalIgnoreCase);

            results.Errors = exceptions.ToArray();
            results.Sets = sets;

            return results;
        }

        private async Task<List<SetVersionLevelOfDetail>> ExtractDetailLevels(SetMetadataContract setMetadata, Uri baseUrl)
        {
            List<SetVersionLevelOfDetail> detailLevels = new List<SetVersionLevelOfDetail>();
            foreach (int detailLevel in Enumerable.Range(setMetadata.MinimumLod, setMetadata.MinimumLod))
            {

                Uri lodMetadataUri = new Uri(baseUrl, "L" + detailLevel + "/metadata.json");
                CubeMetadataContract cubeMetadata = await this.Deserialize<CubeMetadataContract>(lodMetadataUri);

                OcTree<CubeBounds> octree = MetadataLoader.Load(cubeMetadata);
                octree.UpdateTree();

                Vector3 cubeBounds = cubeMetadata.SetSize;

                ExtentsContract worldBounds = cubeMetadata.WorldBounds;

                SetVersionLevelOfDetail currentSetLevelOfDetail = new SetVersionLevelOfDetail();
                currentSetLevelOfDetail.Metadata = lodMetadataUri;
                currentSetLevelOfDetail.Number = detailLevel;
                currentSetLevelOfDetail.Cubes = octree;
                currentSetLevelOfDetail.WorldBounds = new BoundingBox(
                    new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin),
                    new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax));
                currentSetLevelOfDetail.SetSize = new Vector3(cubeBounds.X, cubeBounds.Y, cubeBounds.Z);
                currentSetLevelOfDetail.Name = "L" + detailLevel.ToString(CultureInfo.InvariantCulture);

                currentSetLevelOfDetail.TextureTemplate = new Uri(lodMetadataUri, "texture/{x}_{y}.jpg");
                currentSetLevelOfDetail.ModelTemplate = new Uri(lodMetadataUri,"{x}_{y}_{z}.obj");

                currentSetLevelOfDetail.TextureSetSize = cubeMetadata.TextureSetSize;

                detailLevels.Add(currentSetLevelOfDetail);
            }
            return detailLevels;
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

        protected virtual Uri TransformUri(Uri sourceUri)
        {
            return sourceUri;
        }

        private T DeserializeStream<T>(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader jr = new JsonTextReader(sr))
            {
                return new JsonSerializer().Deserialize<T>(jr);
            }
        }

        private Dictionary<string, SetVersion> GenerateVersionMap(IGrouping<string, SetVersion> setVersions)
        {
            return setVersions.ToDictionary(v => v.Version, v => v, StringComparer.OrdinalIgnoreCase);
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