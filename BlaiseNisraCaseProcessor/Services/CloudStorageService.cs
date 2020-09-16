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

        public IEnumerable<string> GetFilesFromBucket()
        {
            var filesInBucket = _storageClientProvider.GetAvailableFilesFromBucket().ToList();

            if (!filesInBucket.Any())
            {
                _logger.Info($"No files available on bucket '{_configurationProvider.BucketName}'");
                _storageClientProvider.Dispose();

                return new List<string>();
            }

            var localProcessFolder = GetLocalProcessFolder();
            var filesDownloadedFromBucket = new List<string>();

            foreach (var file in filesInBucket)
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
                var fileName = _fileSystem.Path.GetFileName(processedFile);
                _storageClientProvider.MoveFileToProcessedFolder(fileName);
                _logger.Info($"Moved file '{fileName}' into the processed bucket folder");
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
