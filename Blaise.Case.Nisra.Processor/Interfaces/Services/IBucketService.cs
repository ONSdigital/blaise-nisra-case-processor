using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.Interfaces.Services
{
    public interface IBucketService
    {
        IEnumerable<string> GetListOfAvailableFilesInBucket();

        IEnumerable<string> DownloadFilesFromBucket(IList<string> files);

        void MoveProcessedFilesToProcessedFolder(IList<string> processedFiles);
    }
}