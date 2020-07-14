using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class CloudBucketFileService : ICloudBucketFileService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IStorageClientProvider _storageClient;
        private readonly IFileSystem _fileSystem;

        public CloudBucketFileService(
            ILog logger,
            IConfigurationProvider configurationProvider,
            IStorageClientProvider storageClient, 
            IFileSystem fileSystem)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _storageClient = storageClient;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> GetFilesFromBucket()
        {
            var bucketName = _configurationProvider.BucketName;
            var storageClient = _storageClient.GetStorageClient();

            var availableFilesInBucket = storageClient.ListObjects(bucketName, "");

            if (!availableFilesInBucket.Any())
            {
                _logger.Info($"No files available on bucket '{bucketName}'");
                return new List<string>();
            }

            var filesToProcess = GetListOfFilesToProcess(availableFilesInBucket).ToList();
            var localProcessFolder = GetLocalProcessFolder();

            var filesDownloadedFromBucket = new List<string>();

            foreach (var file in filesToProcess)
            {
                _logger.Info($"Processing file '{file.Name}'");
                var filePath = $"{localProcessFolder}/{file.Name}";
                
                using (var fileStream = _fileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
                {
                    storageClient.DownloadObject(bucketName, file.Name, fileStream);
                }

                filesDownloadedFromBucket.Add(filePath);
            }

            storageClient.Dispose();

            return filesDownloadedFromBucket;
        }

        private string GetLocalProcessFolder()
        {
            var localProcessFolder = _configurationProvider.LocalProcessFolder;
            _fileSystem.Directory.CreateDirectory(localProcessFolder);

            return localProcessFolder;
        }

        private IEnumerable<Google.Apis.Storage.v1.Data.Object> GetListOfFilesToProcess(IEnumerable<Google.Apis.Storage.v1.Data.Object> availableFiles)
        {
            var fileNamesToIgnore = _configurationProvider.IgnoreFilesInBucketList;

            return 
                from file in availableFiles 
                from fileName in fileNamesToIgnore 
                where !file.Name.ToLower().Contains(fileName.ToLower()) 
                select file;
        }
    }
}
