using System.Configuration;
using BlaiseNisraCaseProcessor.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Providers;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string TopicId => ConfigurationManager.AppSettings["TopicId"];

        public string BucketName => ConfigurationManager.AppSettings["BucketName"];

        public string CloudStorageKey => ConfigurationManager.AppSettings["CloudStorageKey"];
    }
}
