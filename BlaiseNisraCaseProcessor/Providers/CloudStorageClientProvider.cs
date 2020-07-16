using System.IO.Abstractions;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class CloudStorageClientProvider : StorageClientProvider
    {
        private StorageClient _storageClient;

        public CloudStorageClientProvider(
            IConfigurationProvider configurationProvider,
            IFileSystem fileSystem) : base(
            configurationProvider, 
            fileSystem)
        {
        }


        protected override StorageClient GetStorageClient()
        {
            var client = _storageClient;

            if (client != null)
            {
                return client;
            }

            return (_storageClient = StorageClient.Create());
        }

        protected override void DisposeStorageClient()
        {
            _storageClient?.Dispose();
            _storageClient = null;
        }
    }
}
