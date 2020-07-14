using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services.Files
{
    public interface ICloudBucketFileService
    {
        IEnumerable<string> GetFilesFromBucket();
    }
}