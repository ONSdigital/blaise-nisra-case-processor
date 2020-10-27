﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaiseNisraCaseProcessor.Providers;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Providers
{
    public class LocalConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        [Test]
        public void Given_I_Call_ProjectId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.ProjectId;

            //assert
            Assert.AreEqual("ProjectIdTest", result);
        }

        [Test]
        public void Given_I_Call_SubscriptionId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.SubscriptionId;

            //assert
            Assert.AreEqual("SubscriptionIdTest", result);
        }

        [Test]
        public void Given_I_Call_BucketName_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.BucketName;

            //assert
            Assert.AreEqual("BucketNameTest", result);
        }

        [Test]
        public void Given_I_Call_CloudProcessedFolder_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.CloudProcessedFolder;

            //assert
            Assert.AreEqual("CloudProcessedFolderTest", result);
        }

        [Test]
        public void Given_I_Call_LocalProcessFolder_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.LocalProcessFolder;

            //assert
            Assert.AreEqual("LocalProcessFolderTest", result);
        }

        [Test]
        public void Given_I_Call_VmName_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.VmName;

            //assert
            Assert.AreEqual("VmNameTest", result);
        }

        [Test]
        public void Given_I_Call_IgnoreFilesInBucketList_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.IgnoreFilesInBucketList;

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("processed"));
            Assert.IsTrue(result.Contains("audit"));
        }

        [Test]
        public void Given_I_Call_DeadletterTopicId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.DeadletterTopicId;

            //assert
            Assert.AreEqual("DeadletterTopicIdTest", result);
        }
    }
}
