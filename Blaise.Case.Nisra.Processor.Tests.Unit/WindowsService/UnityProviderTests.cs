using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.WindowsService.Ioc;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.WindowsService
{
    public class UnityProviderTests
    {
        [Test]
        public void Given_I_Create_A_New_Instance_Of_UnityProvider_Then_No_Exceptions_Are_Thrown()
        {
            //act && assert
            Assert.DoesNotThrow(() =>
            {
                var unityProvider = new UnityProvider();
  
            });
        }

        [Test]
        public void
            Given_I_Create_A_New_Instance_Of_UnityProvider_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange
            var sut = new UnityProvider();

            //act
            var result = sut.Resolve<IMessageTriggerHandler>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IMessageTriggerHandler>(result);
        }

        [Test]
        public void
            Given_I_Create_A_New_Instance_Of_IImportNisraDataFileService_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange
            var sut = new UnityProvider();

            //act
            var result = sut.Resolve<IImportNisraDataFileService>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IImportNisraDataFileService>(result);
        }
    }
}
