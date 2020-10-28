using System;
using System.Collections.Generic;
using System.Linq;
using Nisra.Case.Processor.Extensions;
using Nisra.Case.Processor.Interfaces.Providers;

namespace Nisra.Case.Processor.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => GetVariable("ENV_PROJECT_ID");
        
        public string SubscriptionId => GetVariable("ENV_NCP_SUB_SUBS");

        public string DeadletterTopicId => GetVariable("ENV_DEADLETTER_TOPIC");

        public string LocalProcessFolder => GetVariable("ENV_NCP_LOCAL_PROCESS_DIR");

        public string CloudProcessedFolder => GetVariable("ENV_NCP_CLOUD_PROCESS_DIR");

        public string BucketName => GetVariable("ENV_NCP_BUCKET_NAME");

        public IList<string> IgnoreFilesInBucketList
        {
            get
            {
                var filesToIgnore = GetVariable("ENV_NCP_IGNORE_FILES_IN_BUCKET_LIST");

                return string.IsNullOrWhiteSpace(filesToIgnore) 
                    ? new List<string>() 
                    : filesToIgnore.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            }
        }

        public string VmName => GetVariable("VmName");

        private static string GetVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

            value.ThrowExceptionIfNull(variableName);

            return value;
        }
    }
}
