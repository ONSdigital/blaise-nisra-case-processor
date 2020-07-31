using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace BlaiseNisraCaseProcessor.Providers
{
    public abstract class StorageClientProvider : IStorageClientProvider
    {
        protected readonly IConfigurationProvider ConfigurationProvider;
        protected readonly IFileSystem FileSystem;

        protected readonly string BucketName;
        protected readonly string ProcessedFolder;

        protected StorageClientProvider(
            IConfigurationProvider configurationProvider,
            IFileSystem fileSystem)
        {
            ConfigurationProvider = configurationProvider;
            FileSystem = fileSystem;

            BucketName = ConfigurationProvider.BucketName;
            ProcessedFolder = configurationProvider.CloudProcessedFolder;
        }

        protected abstract StorageClient GetStorageClient();

        protected abstract void DisposeStorageClient();


        public IEnumerable<string> GetAvailableFilesFromBucket()
        {
            var storageClient = GetStorageClient();
            var availableFilesInBucket = storageClient.ListObjects(BucketName, "");

            return !availableFilesInBucket.Any()
                ? new List<string>()
                : GetListOfFilesToProcess(availableFilesInBucket).ToList();
        }

        public void Download(string fileName, string filePath)
        {
            var storageClient = GetStorageClient();
            using (var fileStream = FileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
            {
                storageClient.DownloadObject(BucketName, fileName, fileStream);
            }
        }

        public void MoveFileToProcessedFolder(string fileName)
        {
            var storageClient = GetStorageClient();
            foreach (var storageObject in storageClient.ListObjects(BucketName, ""))
            {
                var storageObjectName = FileSystem.Path.GetFileName(storageObject.Name);

                if (!string.Equals(storageObjectName, fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var storageObjectPath = FileSystem.Path.GetDirectoryName(storageObject.Name).Replace("\\", "/");
                var processedPath = $"{storageObjectPath}/{ProcessedFolder}/{storageObjectName}";
                storageClient.CopyObject(BucketName, storageObject.Name, BucketName, processedPath);
                storageClient.DeleteObject(BucketName, storageObject.Name);

                return;
            }
        }

        public void Dispose()
        {
            DisposeStorageClient();
        }

        private IEnumerable<string> GetListOfFilesToProcess(IEnumerable<Google.Apis.Storage.v1.Data.Object> availableFiles)
        {
            var fileNamesToIgnore = ConfigurationProvider.IgnoreFilesInBucketList;
            var filesToProcess = availableFiles.Select(f => f.Name).ToList();

            foreach (var fileNameToIgnore in fileNamesToIgnore)
            {
                filesToProcess.RemoveAll(f => f.Contains(fileNameToIgnore));
            }

            return filesToProcess;
        }
    }
}
