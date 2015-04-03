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
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using CubeServer.Contracts;
    using CubeServer.DataAccess.Json;
    using CubeServer.Model;
    using Newtonsoft.Json;

    public class UriStorage : ICubeStorage, IDisposable
    {
        protected string storageRoot;

        private bool disposed = false;
        private RevolvingState<LoaderResults> loadedSetData = new RevolvingState<LoaderResults>();
        private Thread loaderThread;
        private ManualResetEvent onExit = new ManualResetEvent(false);
        private AutoResetEvent onLoad = new AutoResetEvent(false);

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
                return new VersionResultContract[] { };
            }

            Set set;

            if (!setData.Sets.TryGetValue(setId, out set))
            {
                return new VersionResultContract[] { };
            }

            return set.Versions.Select(v => new VersionResultContract { Name = "v" + v.Number.ToString(CultureInfo.InvariantCulture) });
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

        public Task<StorageStream> GetTextureStream(string setId, string version, string detail, string textureid)
        {
            throw new NotImplementedException();
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

                    Set currentSet = new Set { SourceUri = setMetadataUri, Name = set.Name };

                    List<SetVersion> versions = new List<SetVersion>();
                    foreach (int version in Enumerable.Range(setMetadata.MinimumViewport, setMetadata.MaximumViewport))
                    {
                        string versionMetadata = setMetadata.MetadataTemplate.Replace("{v}", version.ToString(CultureInfo.InvariantCulture));
                        Uri versionMetadataUri = new Uri(setMetadataUri, versionMetadata);

                        OcTree<CubeBounds> octree = await this.Get(versionMetadataUri, MetadataLoader.Load);
                        octree.UpdateTree();

                        SetVersion currentSetVersion = new SetVersion { Cubes = octree, Number = version };

                        versions.Add(currentSetVersion);
                    }
                    currentSet.Versions = versions.OrderBy(v => v.Number).ToArray();
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