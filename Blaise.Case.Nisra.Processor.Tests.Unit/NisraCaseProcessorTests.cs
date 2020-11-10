using Blaise.Case.Nisra.Processor.WindowsService;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit
{
    public class NisraCaseProcessorTests
    {
        [Test]
        public void Given_I_Create_A_New_Instance_Of_BlaiseNisraCaseProcessor_Then_No_Exceptions_Are_Thrown()
        {
            //act && assert
            Assert.DoesNotThrow(() =>
            {
                var blaiseNisraCaseProcessor = new BlaiseNisraCaseProcessor();
            });
        }

        [Test]
        public void Given_I_Create_A_New_Instance_Of_BlaiseNisraCaseProcessor_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange

            //act
            var result = new BlaiseNisraCaseProcessor();

            //assert
            Assert.NotNull(result.InitialiseService);
        }
    }
}
