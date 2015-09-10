// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="UriStorage.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.DataAccess
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
    using Microsoft.Xna.Framework;
    using Newtonsoft.Json;
    using PyriteServer.Contracts;
    using PyriteServer.DataAccess.Json;
    using PyriteServer.Model;
    using System.IO.Compression;

    public class UriStorage : ICubeStorage, IDisposable
    {
        protected string storageRoot;
        private const string FORMAT_PLACEHOLDER = "{format}";
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

        public Task<StorageStream> GetModelStream(string setId, string versionId, string detail, string xpos, string ypos, string zpos, string format)
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                throw new NotFoundException("setData");
            }

            SetVersion setVersion = setData.FindSetVersion(setId, versionId);
            SetVersionLevelOfDetail lod;
            if(!setVersion.DetailLevels.TryGetValue(detail, out lod))
            {
                throw new NotFoundException("detailLevel");
            }

            ModelFormats modelFormat;
            if (format == null)
            {
                modelFormat = ModelFormats.Ebo;
            }
            else if (!ModelFormats.TryParse(format, true, out modelFormat))
            {
                throw new NotFoundException("format");
            }

            string modelPath = lod.ModelTemplate.ToString();
            modelPath = ExpandCoordinatePlaceholders(modelPath, xpos, ypos, zpos, modelFormat);

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
                setVersion.DetailLevels.Values.Select(
                    l =>
                        new LevelOfDetailContract
                        {
                            Name = l.Name,
                            SetSize = new Vector3Contract(l.SetSize),
                            ModelBounds = new BoundingBoxContract(l.ModelBounds),
                            WorldBounds = new BoundingBoxContract(l.WorldBounds),
                            TextureSetSize = new Vector2Contract(l.TextureSetSize),
                            WorldCubeScaling = new Vector3Contract(l.WorldToCubeRatio),
                            VertexCount = l.VertexCount
                        }).ToArray();

            return result;
        }

        public Task<StorageStream> GetTextureStream(string setId, string versionId, string detail, string xpos, string ypos)
        {
            LoaderResults setData = this.loadedSetData.Get();
            if (setData == null)
            {
                throw new NotFoundException("setData");
            }

            SetVersion setVersion = setData.FindSetVersion(setId, versionId);
            SetVersionLevelOfDetail lod;
            if (!setVersion.DetailLevels.TryGetValue(detail, out lod))
            {
                throw new NotFoundException("detailLevel");
            }

            string texturePath = lod.TextureTemplate.ToString();
            texturePath = texturePath.Replace(X_PLACEHOLDER, xpos);
            texturePath = texturePath.Replace(Y_PLACEHOLDER, ypos);
            return this.GetStorageStreamForPath(texturePath);
        }

        public async Task<LoaderResults> LoadMetadata2()
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

                        SetVersion currentSet = new SetVersion { SourceUri = setMetadataUri, Name = set.Name, Version = version.Name, Material = material };

                        List<SetVersionLevelOfDetail> detailLevels = await this.ExtractDetailLevels2(setMetadata, setMetadataUri);

                        currentSet.DetailLevels = new SortedDictionary<string, SetVersionLevelOfDetail>(detailLevels.ToDictionary(d => d.Name, d => d, StringComparer.OrdinalIgnoreCase));
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

                        SetVersion currentSet = new SetVersion { SourceUri = setMetadataUri, Name = set.Name, Version = version.Name, Material = material };

                        List<SetVersionLevelOfDetail> detailLevels = await this.ExtractDetailLevels(setMetadata, setMetadataUri);

                        currentSet.DetailLevels = new SortedDictionary<string, SetVersionLevelOfDetail>(detailLevels.ToDictionary(d => d.Name, d => d, StringComparer.OrdinalIgnoreCase));
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


        public IEnumerable<int[]> Query(string setId, string versionId, string detail, BoundingBox worldBox)
        {
            SetVersion setVersion = this.loadedSetData.Get().FindSetVersion(setId, versionId);
            return setVersion.Query(detail, worldBox);
        }

        public IEnumerable<QueryDetailContract> Query(string setId, string versionId, string profile, BoundingSphere worldSphere)
        {
            SetVersion setVersion = this.loadedSetData.Get().FindSetVersion(setId, versionId);
            return setVersion.Query(profile, worldSphere);
        }

        public IEnumerable<QueryDetailContract> Query(string setId, string versionId, string boundaryReferenceLoD, Vector3 worldCenter)
        {
            SetVersion setVersion = this.loadedSetData.Get().FindSetVersion(setId, versionId);
            return setVersion.Query(boundaryReferenceLoD, worldCenter);
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

        private static string ExpandCoordinatePlaceholders(string modelPath, object xpos, object ypos, object zpos, ModelFormats format)
        {
            modelPath = modelPath.Replace(X_PLACEHOLDER, xpos.ToString());
            modelPath = modelPath.Replace(Y_PLACEHOLDER, ypos.ToString());
            modelPath = modelPath.Replace(Z_PLACEHOLDER, zpos.ToString());
            modelPath = modelPath.Replace(FORMAT_PLACEHOLDER, format.ToString().ToLower());
            return modelPath;
        }

        private T DeserializeStream<T>(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader jr = new JsonTextReader(sr))
            {
                return new JsonSerializer().Deserialize<T>(jr);
            }
        }

        private async Task<List<SetVersionLevelOfDetail>> ExtractDetailLevels(SetMetadataContract setMetadata, Uri baseUrl)
        {
            List<SetVersionLevelOfDetail> detailLevels = new List<SetVersionLevelOfDetail>();
            foreach (int detailLevel in Enumerable.Range(setMetadata.MinimumLod, setMetadata.MaximumLod - setMetadata.MinimumLod + 1))
            {
                string detailLevelName = "L" + detailLevel;

                Uri lodMetadataUri = new Uri(baseUrl, "L" + detailLevel + "/metadata.json");
                CubeMetadataContract cubeMetadata = await this.Deserialize<CubeMetadataContract>(lodMetadataUri);

                OcTree<CubeBounds> octree = MetadataLoader.Load(cubeMetadata, detailLevelName, new Vector3(1,1,1));
                octree.UpdateTree();

                Vector3 cubeBounds = cubeMetadata.SetSize;

                ExtentsContract worldBounds = cubeMetadata.WorldBounds;
                ExtentsContract virtualWorldBounds = cubeMetadata.VirtualWorldBounds;

                SetVersionLevelOfDetail currentSetLevelOfDetail = new SetVersionLevelOfDetail();
                currentSetLevelOfDetail.Metadata = lodMetadataUri;
                currentSetLevelOfDetail.Number = detailLevel;
                currentSetLevelOfDetail.Cubes = octree;
                currentSetLevelOfDetail.ModelBounds = new BoundingBox(
                    new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin),
                    new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax));

                if (virtualWorldBounds != null)
                {
                    currentSetLevelOfDetail.WorldBounds =
                        new BoundingBox(
                            new Vector3(virtualWorldBounds.XMin, virtualWorldBounds.YMin, virtualWorldBounds.ZMin),
                            new Vector3(virtualWorldBounds.XMax, virtualWorldBounds.YMax, virtualWorldBounds.ZMax));
                }
                else
                {
                    currentSetLevelOfDetail.WorldBounds = new BoundingBox(
                        new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin),
                        new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax));
                }

                currentSetLevelOfDetail.SetSize = new Vector3(cubeBounds.X, cubeBounds.Y, cubeBounds.Z);
                currentSetLevelOfDetail.Name = "L" + detailLevel.ToString(CultureInfo.InvariantCulture);
                currentSetLevelOfDetail.VertexCount = cubeMetadata.VertexCount;

                currentSetLevelOfDetail.TextureTemplate = new Uri(lodMetadataUri, "texture/{x}_{y}.jpg");
                currentSetLevelOfDetail.ModelTemplate = new Uri(lodMetadataUri, "{x}_{y}_{z}.{format}");

                currentSetLevelOfDetail.TextureSetSize = cubeMetadata.TextureSetSize;

                detailLevels.Add(currentSetLevelOfDetail);
            }
            return detailLevels;
        }

        private async Task<List<SetVersionLevelOfDetail>> ExtractDetailLevels2(SetMetadataContract setMetadata, Uri baseUrl)
        {
            OcTree<CubeBounds> ocTree = new OcTree<CubeBounds>();

            List<LoaderSet> lods = new List<LoaderSet>();

            foreach (int detailLevel in Enumerable.Range(setMetadata.MinimumLod, setMetadata.MaximumLod - setMetadata.MinimumLod + 1))
            {
                // Allow exceptions to propagate here for logging in ELMAH
                string detailLevelName = "L" + detailLevel;
                Uri lodMetadataUri = new Uri(baseUrl, detailLevelName + "/metadata.json");
                CubeMetadataContract cubeMetadata = await this.Deserialize<CubeMetadataContract>(lodMetadataUri);
            
                lods.Add(new LoaderSet{CubeMetadataContract = cubeMetadata, Name = detailLevelName, DetailLevel = detailLevel, MetadataUrl = lodMetadataUri});
            }

            Vector3 maxSetSize = new Vector3(0,0,0);
            foreach (var loaderSet in lods)
            {
                if (loaderSet.CubeMetadataContract.SetSize.X > maxSetSize.X)
                {
                    maxSetSize.X = loaderSet.CubeMetadataContract.SetSize.X;
                }

                if (loaderSet.CubeMetadataContract.SetSize.Y > maxSetSize.Y)
                {
                    maxSetSize.Y = loaderSet.CubeMetadataContract.SetSize.Y;
                }

                if (loaderSet.CubeMetadataContract.SetSize.Z > maxSetSize.Z)
                {
                    maxSetSize.Z = loaderSet.CubeMetadataContract.SetSize.Z;
                }
            }

            List<SetVersionLevelOfDetail> detailLevels = new List<SetVersionLevelOfDetail>();
            foreach (LoaderSet loaderSet in lods)
            {
                var cubeMetadata = loaderSet.CubeMetadataContract;

                Vector3 cubeSize = maxSetSize / loaderSet.CubeMetadataContract.SetSize;

                ocTree.Add(MetadataLoader.LoadCubeBounds(loaderSet.CubeMetadataContract, loaderSet.Name, cubeSize));

                Vector3 cubeBounds = cubeMetadata.SetSize;

                ExtentsContract worldBounds = cubeMetadata.WorldBounds;
                ExtentsContract virtualWorldBounds = cubeMetadata.VirtualWorldBounds;

                SetVersionLevelOfDetail currentSetLevelOfDetail = new SetVersionLevelOfDetail();
                currentSetLevelOfDetail.Metadata = loaderSet.MetadataUrl;
                currentSetLevelOfDetail.Number = loaderSet.DetailLevel;
                currentSetLevelOfDetail.Cubes = ocTree;
                currentSetLevelOfDetail.ModelBounds = new BoundingBox(
                    new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin),
                    new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax));

                if (virtualWorldBounds != null)
                {
                    currentSetLevelOfDetail.WorldBounds =
                        new BoundingBox(
                            new Vector3(virtualWorldBounds.XMin, virtualWorldBounds.YMin, virtualWorldBounds.ZMin),
                            new Vector3(virtualWorldBounds.XMax, virtualWorldBounds.YMax, virtualWorldBounds.ZMax));
                }
                else
                {
                    currentSetLevelOfDetail.WorldBounds = new BoundingBox(
                        new Vector3(worldBounds.XMin, worldBounds.YMin, worldBounds.ZMin),
                        new Vector3(worldBounds.XMax, worldBounds.YMax, worldBounds.ZMax));
                }

                currentSetLevelOfDetail.SetSize = new Vector3(cubeBounds.X, cubeBounds.Y, cubeBounds.Z);
                currentSetLevelOfDetail.Name = "L" + loaderSet.DetailLevel.ToString(CultureInfo.InvariantCulture);
                currentSetLevelOfDetail.VertexCount = cubeMetadata.VertexCount;

                currentSetLevelOfDetail.TextureTemplate = new Uri(loaderSet.MetadataUrl, "texture/{x}_{y}.jpg");
                currentSetLevelOfDetail.ModelTemplate = new Uri(loaderSet.MetadataUrl, "{x}_{y}_{z}.{format}");

                currentSetLevelOfDetail.TextureSetSize = cubeMetadata.TextureSetSize;

                detailLevels.Add(currentSetLevelOfDetail);
            }

            ocTree.UpdateTree();

            return detailLevels;
        }

        private Dictionary<string, SetVersion> GenerateVersionMap(IGrouping<string, SetVersion> setVersions)
        {
            return setVersions.ToDictionary(v => v.Version, v => v, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<StorageStream> GetStorageStreamForPath(string path)
        {
            Uri targetUri = new Uri(path);
            Trace.WriteLine(targetUri, "UriStorage::GetStorageStreamFromPath");
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


            LoaderResults lkg;
            if (LKGSerializer.TryLoad(out lkg))
            {
                this.loadedSetData.Set(lkg);
                this.onLoad.Set();
            }

            while (!this.onExit.WaitOne(0))
            {
                if (DateTime.Now > next)
                {
                    LoaderResults results = this.LoadMetadata().Result;
                    if (results.Errors.Length == 0)
                    {
                        // materialize data as last known good in temp directory
                        LKGSerializer.TrySave(results);
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

        private static class LKGSerializer
        {
            private static string filename;
            private static JsonSerializer serializer;

            static LKGSerializer()
            {
                LKGSerializer.filename = Path.Combine(Path.GetTempPath(), "pyriteserver_lkg.gzip");
                LKGSerializer.serializer = JsonSerializer.Create();
            }

            public static bool TryLoad(out LoaderResults results)
            {
                results = null;

                try
                {
                    if(!File.Exists(LKGSerializer.filename))
                    {
                        return false;
                    }

                    using (FileStream stream = new FileStream(LKGSerializer.filename, FileMode.Open, FileAccess.Read))
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    using (StreamReader streamReader = new StreamReader(gzip))
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        results = serializer.Deserialize<LoaderResults>(reader);
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return false;
                }

                return true;
            }

            public static bool TrySave(LoaderResults results)
            {
                try
                {
                    using (FileStream stream = new FileStream(LKGSerializer.filename, FileMode.Create, FileAccess.ReadWrite))
                    using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress))
                    using (StreamWriter writer = new StreamWriter(gzip))
                    {
                        serializer.Serialize(writer, results);
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return false;
                }

                return true;
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

        private struct LoaderSet
        {
            internal CubeMetadataContract CubeMetadataContract { get; set; }
            internal string Name { get; set; }
            internal int DetailLevel { get; set; }
            internal Uri MetadataUrl { get; set; }
        }
    }
}