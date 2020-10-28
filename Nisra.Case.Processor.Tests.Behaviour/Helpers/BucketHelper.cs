using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Google.Cloud.Storage.V1;

namespace Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class BucketHelper
    {
        public BucketHelper()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"]);
        }

        public void UploadToBucket(string filePath, string bucketName)
        {
            var fileName = Path.GetFileName(filePath);
            var bucket = StorageClient.Create();
            var bucketFilePath = $"OPN/AutomatedTests/{fileName}";

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bucket.UploadObject(bucketName, bucketFilePath, null, fileStream);
            }
        }

        public bool FilesHaveBeenProcessed(string bucketName)
        {
            var storageClient = StorageClient.Create();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");

            //get all objects that are not folders
            var availableFiles = availableObjectsInBucket.Where(f => f.Size > 0).Select(f => f.Name).ToList();
            availableFiles.RemoveAll(f => f.Contains(ConfigurationManager.AppSettings["IgnoreFilesInBucketList"]));

            return availableFiles.Count == 0;
        }
    }
}
