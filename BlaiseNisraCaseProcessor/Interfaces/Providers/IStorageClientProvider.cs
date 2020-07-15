
using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        IEnumerable<string> GetAvailableFilesFromBucket();

        void Download(string fileName, string filePath);

        void MoveFileToProcessedFolder(string fileName);

        void Dispose();
    }
}
