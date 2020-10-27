using System;
using System.Collections.Generic;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => Environment.GetEnvironmentVariable("ENV_PROJECT_ID", EnvironmentVariableTarget.Machine);

        public string SubscriptionId => Environment.GetEnvironmentVariable("ENV_NCP_SUB_SUBS", EnvironmentVariableTarget.Machine);

        public string DeadletterTopicId => Environment.GetEnvironmentVariable("ENV_DEADLETTER_TOPIC", EnvironmentVariableTarget.Machine);

        public string LocalProcessFolder => Environment.GetEnvironmentVariable("ENV_NCP_LOCAL_PROCESS_DIR", EnvironmentVariableTarget.Machine);

        public string CloudProcessedFolder => Environment.GetEnvironmentVariable("ENV_NCP_CLOUD_PROCESS_DIR", EnvironmentVariableTarget.Machine);

        public string BucketName => Environment.GetEnvironmentVariable("ENV_NCP_BUCKET_NAME", EnvironmentVariableTarget.Machine);

        public IList<string> IgnoreFilesInBucketList
        {
            get
            {
                var filesToIgnore = Environment.GetEnvironmentVariable("ENV_NCP_IGNORE_FILES_IN_BUCKET_LIST", EnvironmentVariableTarget.Machine);

                return string.IsNullOrWhiteSpace(filesToIgnore) 
                    ? new List<string>() 
                    : filesToIgnore.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            }
        }

        public string VmName => Environment.MachineName;
    }
}
