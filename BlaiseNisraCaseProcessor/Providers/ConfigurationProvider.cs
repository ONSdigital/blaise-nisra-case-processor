using System.Collections.Generic;
using System.Configuration;
using BlaiseNisraCaseProcessor.Interfaces.Providers;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string TopicId => ConfigurationManager.AppSettings["TopicId"];

        public string LocalProcessFolder => ConfigurationManager.AppSettings["LocalProcessFolder"];

        public string BucketName => ConfigurationManager.AppSettings["BucketName"];

        public IList<string> IgnoreFilesInBucketList
        {
            get
            {
                var filesToIgnore = ConfigurationManager.AppSettings["IgnoreFilesInBucketList"];

                if (string.IsNullOrWhiteSpace(filesToIgnore))
                {
                    return new List<string>();
                }

                return filesToIgnore.Split(',');
            }
        } 

        public string CloudStorageKey => ConfigurationManager.AppSettings["CloudStorageKey"];
    }
}
