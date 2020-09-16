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
            var availableObjectsInBucket = storageClient.ListObjects(BucketName, "");

            //get all objects that are not folders
            var availableFiles = availableObjectsInBucket.Where(f => f.Size > 0).Select(f => f.Name).ToList();

            return availableFiles.Any()
                ? RemovedFilesFromIgnoreList(availableFiles).ToList()
                : new List<string>();
        }

        public void Download(string fileName, string filePath)
        {
            var storageClient = GetStorageClient();
            using (var fileStream = FileSystem.FileStream.Create(filePath, FileMode.OpenOrCreate))
            {
                storageClient.DownloadObject(BucketName, fileName, fileStream);
            }
        }

        public void MoveFileToProcessedFolder(string file)
        {
            var storageClient = GetStorageClient();
            foreach (var storageObject in storageClient.ListObjects(BucketName, ""))
            {
                if (!string.Equals(storageObject.Name, file, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var fileName = FileSystem.Path.GetFileName(file);
                var filePath = FileSystem.Path.GetDirectoryName(file).Replace("\\", "/");
                var processedPath = $"{filePath}/{ProcessedFolder}/{fileName}";
                
                storageClient.CopyObject(BucketName, storageObject.Name, BucketName, processedPath);
                storageClient.DeleteObject(BucketName, storageObject.Name);

                return;
            }
        }

        public void Dispose()
        {
            DisposeStorageClient();
        }

        private IEnumerable<string> RemovedFilesFromIgnoreList(List<string> availableFiles)
        {
            var fileNamesToIgnore = ConfigurationProvider.IgnoreFilesInBucketList;

            foreach (var fileNameToIgnore in fileNamesToIgnore)
            {
                availableFiles.RemoveAll(f => f.Contains(fileNameToIgnore));
            }

            return availableFiles;
        }
    }
}
