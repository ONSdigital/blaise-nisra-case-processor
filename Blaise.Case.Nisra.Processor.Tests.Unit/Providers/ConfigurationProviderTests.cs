using System.Configuration;
using Blaise.Case.Nisra.Processor.Providers;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.Providers
{
    public class ConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        [Test]
        public void Given_I_Call_ProjectId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.ProjectId;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_PROJECT_ID'", exception.Message);
        }

        [Test]
        public void Given_I_Call_SubscriptionId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.SubscriptionId;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_NCP_SUB_SUBS'", exception.Message);
        }

        [Test]
        public void Given_I_Call_BucketName_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.BucketName;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_NCP_BUCKET_NAME'", exception.Message);
        }

        [Test]
        public void Given_I_Call_CloudProcessedFolder_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.CloudProcessedFolder;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_NCP_CLOUD_PROCESS_DIR'", exception.Message);
        }

        [Test]
        public void Given_I_Call_LocalProcessFolder_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.LocalProcessFolder;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_NCP_LOCAL_PROCESS_DIR'", exception.Message);
        }

        [Test]
        public void Given_I_Call_IgnoreFilesInBucketList_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.IgnoreFilesInBucketList;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_NCP_IGNORE_FILES_IN_BUCKET_LIST'", exception.Message);
        }

        [Test]
        public void Given_I_Call_DeadletterTopicId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.DeadletterTopicId;
            });
            Assert.AreEqual("No value found for environment variable 'ENV_DEADLETTER_TOPIC'", exception.Message);
        }
    }
}

