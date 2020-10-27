using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class CloudStorageClientProvider : IStorageClientProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFileSystem _fileSystem;

        private readonly string _bucketName;
        private readonly string _processedFolder;

        private StorageClient _storageClient;

        public CloudStorageClientProvider(
            IConfigurationProvider configurationProvider,
            IFileSystem fileSystem)
        {
            _configurationProvider = configurationProvider;
            _fileSystem = fileSystem;

            _bucketName = _configurationProvider.BucketName;
            _processedFolder = configurationProvider.CloudProcessedFolder;
        }

        public StorageClient GetStorageClient()
        {
            var client = _storageClient;

            if (client != null)
            {
                return client;
            }

            return (_storageClient = StorageClient.Create());
        }

        public void DisposeStorageClient()
        {
            _storageClient?.Dispose();
            _storageClient = null;
        }
        
        public IEnumerable<string> GetAvailableFilesFromBucket()
        {
            var storageClient = GetStorageClient();
            var availableObjectsInBucket = storageClient.ListObjects(_bucketName, "");

            //get all objects that are not folders
            var availableFiles = availableObjectsInBucket.Where(f => f.Size > 0).Select(f => f.Name).ToList();

            return availableFiles.Any()
                ? RemovedFilesFromIgnoreList(availableFiles).ToList()
                : new List<string>();
        }

        public void Download(string fileName, string filePath)
        {
            var storageClient = GetStorageClient();
            using (var fileStream = _fileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
            {
                storageClient.DownloadObject(_bucketName, fileName, fileStream);
            }
        }

        public void MoveFileToProcessedFolder(string file)
        {
            var storageClient = GetStorageClient();
            foreach (var storageObject in storageClient.ListObjects(_bucketName, ""))
            {
                if (!string.Equals(storageObject.Name, file, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var fileName = _fileSystem.Path.GetFileName(file);
                var filePath = _fileSystem.Path.GetDirectoryName(file).Replace("\\", "/");
                var processedPath = $"{filePath}/{_processedFolder}/{fileName}";
                
                storageClient.CopyObject(_bucketName, storageObject.Name, _bucketName, processedPath);
                storageClient.DeleteObject(_bucketName, storageObject.Name);

                return;
            }
        }

        public void Dispose()
        {
            DisposeStorageClient();
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
    }
}
