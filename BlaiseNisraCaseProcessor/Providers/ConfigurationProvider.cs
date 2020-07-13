using System.Configuration;
using BlaiseNisraCaseProcessor.Interfaces;

namespace BlaiseNisraCaseProcessor.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string TopicId => ConfigurationManager.AppSettings["TopicId"];
    }
}
