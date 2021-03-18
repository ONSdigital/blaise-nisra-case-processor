
namespace Blaise.Case.Nisra.Processor.CloudStorage.Interfaces
{
    public interface IStorageService
    {
        string GetInstrumentFileFromBucket(string instrumentName, string bucketPath);

        void DeleteDownloadedFiles();
    }
}