namespace PyriteServer.DataAccess
{
    using System.Configuration;
    using PyriteServer.Contracts;

    /// <summary>Reads account secrets information from Azure AppSettings at runtime</summary>
    public class AzureAppSettingsProvider : ISecretsProvider
    {
        public AzureAppSettingsProvider()
        {
        }

        public string Value
        {
            get
            {
                return ConfigurationManager.AppSettings["Storage"];
            }
        }

        public bool Exists
        {
            get
            {
                return !string.IsNullOrEmpty(Value);
            }
        }
    }
}