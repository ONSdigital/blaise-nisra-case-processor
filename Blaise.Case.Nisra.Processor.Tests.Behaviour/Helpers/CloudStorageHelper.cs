using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class CloudStorageHelper
    {
        private StorageClient _storageClient;
        private static CloudStorageHelper _currentInstance;

        public static CloudStorageHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new CloudStorageHelper());
        }

        public async Task UploadFolderToBucketAsync(string bucketPath, string folderPath)
        {
            var storageClient = await GetStorageClientAsync();
            var filesInFolder = Directory.GetFiles(folderPath);
            foreach (var file in filesInFolder)
            {
                using (var fileStream = File.OpenRead(file))
                {
                    await storageClient.UploadObjectAsync(bucketPath, $"{BlaiseConfigurationHelper.InstrumentName}/{Path.GetFileName(file)}", null, fileStream);
                }
            }          
        }

        public async Task<string> DownloadFromBucketAsync(string bucketPath, string fileName, string destinationFilePath)
        {
            var storageClient = await GetStorageClientAsync();

            if (!Directory.Exists(destinationFilePath))
            {
                Directory.CreateDirectory(destinationFilePath);
            }

            var downloadedFile = Path.Combine(destinationFilePath, fileName);

            using (var fileStream = File.OpenWrite(downloadedFile))
            {
                await storageClient.DownloadObjectAsync(bucketPath, fileName, fileStream);
            }

            return downloadedFile;
        }

        public async Task DeleteFilesInBucketAsync(string bucketName, string bucketPath)
        {
            var storageClient = await GetStorageClientAsync();
            var storageObjects = storageClient.ListObjects(bucketName, $"{bucketPath}/");

            foreach (var storageObject in storageObjects)
            {
                await storageClient.DeleteObjectAsync(bucketName, storageObject.Name);
            }
        }

        public async Task<bool> FilesHaveBeenProcessedAsync(string bucketName)
        {
            var storageClient = await GetStorageClientAsync();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");

            //get all objects that are not folders
            var availableFiles = availableObjectsInBucket.Where(f => f.Size > 0).Select(f => f.Name).ToList();

            return availableFiles.Count == 0;
        }

        private async Task<StorageClient> GetStorageClientAsync()
        {
            var client = _storageClient;

            if (client != null)
            {
                return client;
            }

            return _storageClient = await StorageClient.CreateAsync();
        }
    }
}
