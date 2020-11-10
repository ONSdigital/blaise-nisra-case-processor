using System.Collections.Generic;

namespace Blaise.Case.Nisra.Processor.CloudStorage.Interfaces
{
    public interface IStorageService
    {
        IEnumerable<string> GetListOfAvailableFilesInBucket();

        IEnumerable<string> DownloadFilesFromBucket(IList<string> files);

        void MoveProcessedFilesToProcessedFolder(IList<string> processedFiles);
    }
}