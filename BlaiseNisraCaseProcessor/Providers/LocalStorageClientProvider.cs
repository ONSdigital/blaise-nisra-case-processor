using System.IO;
using System.IO.Abstractions;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class LocalStorageClientProvider : StorageClientProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        private StorageClient _storageClient;

        public LocalStorageClientProvider(
            IConfigurationProvider configurationProvider,
            IFileSystem fileSystem) : base(
            configurationProvider, 
            fileSystem)
        {
            _configurationProvider = configurationProvider;
        }


        protected override StorageClient GetStorageClient()
        {
            if (_storageClient == null)
            {
                var credentialsKey = _configurationProvider.CloudStorageKey;
                var googleCredStream = GoogleCredential.FromStream(File.OpenRead(credentialsKey));
                _storageClient = StorageClient.Create(googleCredStream);
            }

            return _storageClient;
        }
        protected override void DisposeStorageClient()
        {
            _storageClient?.Dispose();
            _storageClient = null;
        }
    }
}
