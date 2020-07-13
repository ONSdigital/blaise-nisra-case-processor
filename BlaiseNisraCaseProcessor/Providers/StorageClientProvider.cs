using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class StorageClientProvider : IStorageClientProvider
    {
        public StorageClient GetStorageClient()
        {
            return StorageClient.Create();
        }
    }
}
