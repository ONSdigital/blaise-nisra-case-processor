using Blaise.Case.Nisra.Processor.Core.Configuration;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.Core
{
    public class ConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        private IConfigurationProvider _sut;

        [SetUp]
        public void SetUpTests()
        {
            _sut = new ConfigurationProvider();
        }

        [Test]
        public void Given_ProjectId_Value_Is_Set_When_I_Call_ProjectId_I_Get_The_Correct_Value_Back()
        {
            //act
            var result = _sut.ProjectId;

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"ProjectIdTest", result);
        }

        [Test]
        public void Given_SubscriptionId_Value_Is_Set_When_I_Call_SubscriptionId_I_Get_The_Correct_Value_Back()
        {
            //act
            var result = _sut.SubscriptionId;

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"SubscriptionIdTest", result);
        }

        [Test]
        public void Given_DeadletterTopicId_Value_Is_Set_When_I_Call_DeadletterTopicId_I_Get_The_Correct_Value_Back()
        {
            //act
            var result = _sut.DeadletterTopicId;

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"DeadletterTopicIdTest", result);
        }

        [Test]
        public void Given_LocalTempFolder_Value_Is_Set_When_I_Call_LocalTempFolder_I_Get_The_Correct_Value_Back()
        {
            //act
            var result = _sut.LocalTempFolder;

            //assert
            Assert.NotNull(result);
            Assert.True(result.StartsWith(@"LocalTempFolderTest"));
        }

        [Test]
        public void Given_I_Call_LocalProcessFolder_I_Get_A_Unique_Path_Back_Each_Time()
        {
            //act
            var result1 = _sut.LocalTempFolder;
            var result2 = _sut.LocalTempFolder;

            //assert
            Assert.AreNotEqual(result1, result2);
        }

        [Test]
        public void Given_BucketName_Value_Is_Set_When_I_Call_BucketName_I_Get_The_Correct_Value_Back()
        {
            //act
            var result = _sut.BucketName;

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(@"BucketNameTest", result);
        }
    }
}

