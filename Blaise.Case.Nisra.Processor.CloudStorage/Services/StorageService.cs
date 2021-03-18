using System;
using System.IO.Abstractions;
using System.Linq;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;

namespace Blaise.Case.Nisra.Processor.CloudStorage.Services
{
    public class StorageService : IStorageService
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICloudStorageClientProvider _cloudStorageClient;
        private readonly IFileSystem _fileSystem;
        private readonly ILoggingService _loggingService;

        public StorageService(
            IConfigurationProvider configurationProvider,
            ICloudStorageClientProvider cloudStorageClient,
            IFileSystem fileSystem,
            ILoggingService loggingService)
        {
            _configurationProvider = configurationProvider;
            _cloudStorageClient = cloudStorageClient;
            _fileSystem = fileSystem;
            _loggingService = loggingService;
        }
        public string GetInstrumentFileFromBucket(string instrumentName, string bucketPath)
        {
            var bucketFiles = _cloudStorageClient.GetListOfFiles(_configurationProvider.BucketName, bucketPath).ToList();

            if (!bucketFiles.Any())
            {
                throw new Exception($"No files were found for bucket path '{bucketPath}' in bucket '{_configurationProvider.BucketName}'");
            }

            _loggingService.LogInfo($"Attempting to Download '{bucketFiles.Count}' files from bucket '{_configurationProvider.BucketName}'");

            foreach (var file in bucketFiles)
            {
                DownloadFileFromBucket(_configurationProvider.BucketName, file, _configurationProvider.LocalTempFolder);
            }

            _loggingService.LogInfo($"Downloaded '{bucketFiles.Count}' files from bucket '{_configurationProvider.BucketName}'");

            return GetDatabaseFile(instrumentName, _configurationProvider.LocalTempFolder);
        }

        public void DeleteDownloadedFiles()
        {
          _fileSystem.Directory.Delete(_configurationProvider.LocalTempFolder, true);
        }

        public string DownloadFileFromBucket(string bucketName, string bucketFilePath, string tempFilePath)
        {
            if (!_fileSystem.Directory.Exists(tempFilePath))
            {
                _fileSystem.Directory.CreateDirectory(tempFilePath);
            }

            var fileName = _fileSystem.Path.GetFileName(bucketFilePath);
            var downloadedFile = _fileSystem.Path.Combine(tempFilePath, fileName);

            _cloudStorageClient.Download(bucketName, bucketFilePath, downloadedFile);

            _loggingService.LogInfo($"Downloaded '{fileName}' from bucket '{bucketName}' to '{tempFilePath}'");

            return downloadedFile;
        }

        public string GetDatabaseFile(string instrumentName, string filePath)
        {
            return _fileSystem.Path.Combine(filePath, $"{instrumentName}.bdix");
        }
    }
}
