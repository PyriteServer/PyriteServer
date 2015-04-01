// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Global.asax.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http.Formatting;
    using System.Web;
    using System.Web.Http;
    using CubeServer.DataAccess;

    public class WebApiApplication : HttpApplication
    {
        private static readonly UriStorage storage = new UriStorage("http://cubeserver.blob.core.windows.net/sets/demosets.json");
        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this.disposed)
            {
                return;
            }

            if (storage != null)
            {
                storage.Dispose();
            }
            base.Dispose();

            this.disposed = true;
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            GlobalConfiguration.Configuration.EnsureInitialized();

            string storagePath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ".\\Data");
            storagePath = Path.GetFullPath(storagePath);
            Trace.WriteLine(String.Format("Storage path: {0}", storagePath));

            Dependency.Storage = storage;
        }
    }
}