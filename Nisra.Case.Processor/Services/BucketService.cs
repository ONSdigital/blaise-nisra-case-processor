using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using log4net;
using Nisra.Case.Processor.Interfaces.Providers;
using Nisra.Case.Processor.Interfaces.Services;

namespace Nisra.Case.Processor.Services
{
    public class BucketService : IBucketService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IStorageClientProvider _storageClientProvider;
        private readonly IFileSystem _fileSystem;

        public BucketService(
            ILog logger,
            IConfigurationProvider configurationProvider,
            IStorageClientProvider storageClientProvider, 
            IFileSystem fileSystem)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _storageClientProvider = storageClientProvider;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> GetListOfAvailableFilesInBucket()
        {
            var filesAvailable = _storageClientProvider.GetListOfFilesInBucket(_configurationProvider.BucketName).ToList();
            var filteredFileList = RemovedFilesFromIgnoreList(filesAvailable);

            _storageClientProvider.Dispose();

            return filteredFileList;
        }

        public IEnumerable<string> DownloadFilesFromBucket(IList<string> files)
        {
            if (!files.Any())
            {
                return new List<string>();
            }

            var localProcessFolder = GetLocalProcessFolder();
            var filesDownloadedFromBucket = new List<string>();

            foreach (var file in files)
            {
                _logger.Info($"Processing file '{file}'");
                var fileName = _fileSystem.Path.GetFileName(file);
                var filePath = _fileSystem.Path.Combine(localProcessFolder, fileName);

                _storageClientProvider.Download(_configurationProvider.BucketName, file, filePath);

                filesDownloadedFromBucket.Add(filePath);
            }

            _storageClientProvider.Dispose();

            return filesDownloadedFromBucket;
        }

        public void MoveProcessedFilesToProcessedFolder(IList<string> processedFiles)
        {
            if (!processedFiles.Any())
            {
                _storageClientProvider.Dispose();
                return;
            }

            foreach (var processedFile in processedFiles)
            {
                _storageClientProvider.MoveFileToProcessedFolder(_configurationProvider.BucketName, processedFile);
                
                _logger.Info($"Moved file '{processedFile}' into the processed bucket folder");
            }

            _storageClientProvider.Dispose();
        }

        private IEnumerable<string> RemovedFilesFromIgnoreList(List<string> availableFiles)
        {
            var fileNamesToIgnore = _configurationProvider.IgnoreFilesInBucketList;

            foreach (var fileNameToIgnore in fileNamesToIgnore)
            {
                availableFiles.RemoveAll(f => f.Contains(fileNameToIgnore));
            }

            return availableFiles;
        }

        private string GetLocalProcessFolder()
        {
            var localProcessFolder = _configurationProvider.LocalProcessFolder;
            _fileSystem.Directory.CreateDirectory(localProcessFolder);

            return localProcessFolder;
        }
    }
}
