using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Providers;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Integration.Cloud
{
    public class StorageClientProviderTests
    {
        [Ignore("")]
        [Test]
        public void Given_Files_Are_Available_In_The_Bucket_When_I_Call_GetAvailableFilesFromBucket_Then_The_Correct_List_Of_Files_Are_Returned()
        {
            //arrange
            var unityProvider = new UnityProvider();
            var sut = unityProvider.Resolve<IStorageClientProvider>();

            //act
            var result = sut.GetAvailableFilesFromBucket();

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count());
        }
    }
}
