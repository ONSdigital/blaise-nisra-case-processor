using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface IBucketService
    {
        IEnumerable<string> GetListOfAvailableFilesInBucket();

        IEnumerable<string> DownloadFilesFromBucket(IEnumerable<string> files);

        void MoveProcessedFilesToProcessedFolder(IEnumerable<string> processedFiles);
    }
}