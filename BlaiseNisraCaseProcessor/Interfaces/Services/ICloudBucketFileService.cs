using System.Collections.Generic;

namespace BlaiseNisraCaseProcessor.Interfaces.Services
{
    public interface ICloudBucketFileService
    {
        IEnumerable<string> GetFilesFromBucket();
    }
}