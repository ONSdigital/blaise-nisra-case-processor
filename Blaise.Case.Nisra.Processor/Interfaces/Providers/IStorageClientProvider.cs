
using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        IEnumerable<string> GetListOfFilesInBucket(string bucketName);

        void Download(string bucketName, string fileName, string filePath);

        void MoveFileToProcessedFolder(string bucketName, string fileName);

        void Dispose();
    }
}
