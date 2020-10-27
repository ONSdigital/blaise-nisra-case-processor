using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class LocalConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string SubscriptionId => ConfigurationManager.AppSettings["SubscriptionId"];

        public string LocalProcessFolder => ConfigurationManager.AppSettings["LocalProcessFolder"];
        public string CloudProcessedFolder => ConfigurationManager.AppSettings["CloudProcessedFolder"];

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

                return filesToIgnore.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            }
        }

        public string VmName => ConfigurationManager.AppSettings["VmName"];

        public string DeadletterTopicId => ConfigurationManager.AppSettings["DeadletterTopicId"];
    }
}
