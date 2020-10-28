using System.Configuration;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class ConfigurationHelper
    {
        public string GoogleApplicationCredentials => GetVariable("GOOGLE_APPLICATION_CREDENTIALS");

        public string ProjectId => GetVariable("ProjectId");

        public string TopicId => GetVariable("TopicId");

        public string LocalProcessFolder => GetVariable("LocalProcessFolder");

        public string BucketName => GetVariable("BucketName");

        public string IgnoreFilesInBucketList => GetVariable("IgnoreFilesInBucketList");

        public string DatabaseFileNameExt => GetVariable("DatabaseFileNameExt");

        public string InstrumentName => GetVariable("InstrumentName");

        public string ServerParkName => GetVariable("ServerParkName");

        private static string GetVariable(string variableName)
        {
            var value = ConfigurationManager.AppSettings["variableName"]; ;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException($"No value found for environment variable '{variableName}'");
            }

            return value;
        }
    }
}
