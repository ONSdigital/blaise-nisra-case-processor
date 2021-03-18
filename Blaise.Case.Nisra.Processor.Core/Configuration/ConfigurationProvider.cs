using System;
using Blaise.Case.Nisra.Processor.Core.Extensions;
using Blaise.Case.Nisra.Processor.Core.Interfaces;

namespace Blaise.Case.Nisra.Processor.Core.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationExtensions.GetEnvironmentVariable("ENV_PROJECT_ID");
        
        public string SubscriptionId => ConfigurationExtensions.GetEnvironmentVariable("ENV_NCP_SUB_SUBS");

        public string DeadletterTopicId => ConfigurationExtensions.GetEnvironmentVariable("ENV_DEADLETTER_TOPIC");

        public string LocalTempFolder => $"{ConfigurationExtensions.GetVariable("TEMP_PATH")}\\{Guid.NewGuid()}";

        public string BucketName => ConfigurationExtensions.GetEnvironmentVariable("ENV_BLAISE_NISRA_BUCKET");
    }
}
