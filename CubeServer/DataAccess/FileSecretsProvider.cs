// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="FileSecretsProvider.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.DataAccess
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using CubeServer.Contracts;

    public class FileSecretsProvider : ISecretsProvider
    {
        private readonly string connectionSecret;

        public FileSecretsProvider(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }

            filename = Path.GetFullPath(filename);
            Trace.WriteLine(filename, "FileSecretsProvider::cctor");

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("Secrets file not found", filename);
            }

            this.connectionSecret = File.ReadAllText(filename).Trim();
        }

        public string Value
        {
            get { return this.connectionSecret; }
        }
    }
}