using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class StorageClientProvider : IStorageClientProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFileSystem _fileSystem;

        private readonly StorageClient _storageClient;
        private readonly string _bucketName;


        public StorageClientProvider(
            IConfigurationProvider configurationProvider, 
            IFileSystem fileSystem)
        {
            _configurationProvider = configurationProvider;
            _fileSystem = fileSystem;
        }

        public StorageClientProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            if (_storageClient == null)
            {
                _storageClient = StorageClient.Create();
            }

            _bucketName = _configurationProvider.BucketName;
        }
        
        public IEnumerable<string> GetAvailableFilesFromBucket()
        {
            var availableFilesInBucket = _storageClient.ListObjects(_bucketName, "");

            return !availableFilesInBucket.Any() 
                ? new List<string>() 
                : GetListOfFilesToProcess(availableFilesInBucket).ToList();
        }

        public void Download(string fileName, string filePath)
        {
            using (var fileStream = _fileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
            {
                _storageClient.DownloadObject(_bucketName, fileName, fileStream);
            }
        }

        public void MoveFileToProcessedFolder(string fileName)
        {
            foreach (var storageObject in _storageClient.ListObjects(_bucketName, ""))
            {
                var storageObjectName = _fileSystem.Path.GetFileName(storageObject.Name);
                var storageObjectPath = _fileSystem.Path.GetDirectoryName(storageObjectName);

                if (string.Equals(storageObjectName, fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var processedPath = $"{storageObjectPath}/Processed/{storageObjectName}";
                    _storageClient.CopyObject(_bucketName, storageObject.Name, _bucketName, processedPath);
                    _storageClient.DeleteObject(_bucketName, storageObject.Name);
                }
            }
        }

        public void Dispose()
        {
            _storageClient?.Dispose();
        }

        private IEnumerable<string> GetListOfFilesToProcess(IEnumerable<Google.Apis.Storage.v1.Data.Object> availableFiles)
        {
            var fileNamesToIgnore = _configurationProvider.IgnoreFilesInBucketList;

            return
                from file in availableFiles
                from fileName in fileNamesToIgnore
                where !file.Name.ToLower().Contains(fileName.ToLower())
                select file.Name;
        }
    }
}
