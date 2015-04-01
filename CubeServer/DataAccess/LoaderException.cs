namespace CubeServer.DataAccess
{
    using System;

    [Serializable]
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
}