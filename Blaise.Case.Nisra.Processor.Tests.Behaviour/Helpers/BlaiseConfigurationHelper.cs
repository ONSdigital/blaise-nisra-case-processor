using Blaise.Case.Nisra.Processor.Tests.Behaviour.Extensions;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public static class BlaiseConfigurationHelper
    {
        public static string GoogleApplicationCredentials => ConfigurationExtensions.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        public static string ServerParkName => ConfigurationExtensions.GetEnvironmentVariable("ServerParkName");
        public static string InstrumentPath => ConfigurationExtensions.GetEnvironmentVariable("InstrumentPath");
        public static string InstrumentName => ConfigurationExtensions.GetEnvironmentVariable("InstrumentName");
        public static string InstrumentExtension => ConfigurationExtensions.GetVariable("PACKAGE_EXTENSION");
        public static string InstrumentPackage => $"{InstrumentPath}//{InstrumentName}.{InstrumentExtension}";
        public static string InstrumentFile => $"{InstrumentName}.{InstrumentExtension}";
        public static string InstrumentPackageBucket => ConfigurationExtensions.GetVariable("ENV_BLAISE_DQS_BUCKET");
        public static string TempPath => ConfigurationExtensions.GetVariable("TEMP_PATH");
        public static string NisraBucket => ConfigurationExtensions.GetVariable("ENV_BLAISE_NISRA_BUCKET");
        public static string ProjectId => ConfigurationExtensions.GetVariable("ENV_PROJECT_ID");
        public static string TopicId => ConfigurationExtensions.GetVariable("ENV_NCP_TOPIC");
    }
}
