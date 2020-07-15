using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface ICloudStorageService
    {
        IEnumerable<string> GetFilesFromBucket();

        void MoveProcessedFilesToProcessedFolder(IList<string> processedFiles);
    }
}