
using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.CloudStorage.Interfaces
{
    public interface ICloudStorageClientProvider
    {
        IEnumerable<string> GetListOfFiles(string bucketName, string bucketPath);
        void Download(string bucketName, string fileName, string destinationFilePath);
    }
}
