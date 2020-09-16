using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IStorageClientProvider _storageClientProvider;
        private readonly IFileSystem _fileSystem;

        public CloudStorageService(
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

        public IList<string> GetAvailableFilesFromBucket()
        {
            var filesAvailable = _storageClientProvider.GetAvailableFilesFromBucket().ToList();
            _storageClientProvider.Dispose();

            return filesAvailable;
        }

        public IList<string> DownloadFilesFromBucket(IList<string> files)
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

                _storageClientProvider.Download(file, filePath);

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
                _storageClientProvider.MoveFileToProcessedFolder(processedFile);
                
                _logger.Info($"Moved file '{processedFile}' into the processed bucket folder");
            }

            _storageClientProvider.Dispose();
        }

        private string GetLocalProcessFolder()
        {
            var localProcessFolder = _configurationProvider.LocalProcessFolder;
            _fileSystem.Directory.CreateDirectory(localProcessFolder);

            return localProcessFolder;
        }
    }
}
