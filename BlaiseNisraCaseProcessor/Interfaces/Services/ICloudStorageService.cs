using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface ICloudStorageService
    {
        IList<string> GetAvailableFilesFromBucket();

        IList<string> DownloadFilesFromBucket(IList<string> files);

        void MoveProcessedFilesToProcessedFolder(IList<string> processedFiles);
    }
}