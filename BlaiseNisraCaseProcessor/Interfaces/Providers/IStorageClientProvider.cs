
using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        IEnumerable<string> GetListOfFilesInBucket(string bucketName);

        void Download(string bucketName, string fileName, string filePath);

        void MoveFileToProcessedFolder(string bucketName, string fileName);

        void Dispose();
    }
}
