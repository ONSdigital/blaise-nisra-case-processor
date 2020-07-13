
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        StorageClient GetStorageClient();
    }
}
