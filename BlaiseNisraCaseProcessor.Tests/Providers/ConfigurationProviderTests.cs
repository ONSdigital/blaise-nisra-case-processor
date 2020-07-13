using BlaiseNisraCaseProcessor.Providers;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Providers
{
    public class ConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        [Test]
        public void Given_I_Call_ProjectId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act
            var result = configurationProvider.ProjectId;

            //assert
            Assert.AreEqual("ProjectIdTest", result);
        }

        [Test]
        public void Given_I_Call_TopicId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act
            var result = configurationProvider.TopicId;

            //assert
            Assert.AreEqual("TopicIdTest", result);
        }
    }
}
