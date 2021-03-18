using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Nisra.Processor.CloudStorage.Providers
{
    public class CloudStorageClientProvider : ICloudStorageClientProvider
    {
        private readonly IFileSystem _fileSystem;

        public CloudStorageClientProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        
        public IEnumerable<string> GetListOfFiles(string bucketName, string bucketPath)
        {
            using (var storageClient = StorageClient.Create())
            {
                var files = new List<string>();
                var storageObjects = storageClient.ListObjects(bucketName, bucketPath);

                foreach (var storageObject in storageObjects)
                {
                    files.Add(storageObject.Name);
                }

                return files;
            }
        }

        public void Download(string bucketName, string fileName, string destinationFilePath)
        {
            using (var storageClient = StorageClient.Create())
            {
                using (var fileStream = _fileSystem.FileStream.Create(destinationFilePath, FileMode.OpenOrCreate))
                {
                    storageClient.DownloadObject(bucketName, fileName, fileStream);
                }
            }
        }
    }
}
