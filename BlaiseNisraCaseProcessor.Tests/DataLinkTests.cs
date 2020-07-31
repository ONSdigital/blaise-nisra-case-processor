using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatNeth.Blaise.API.DataLink;

namespace BlaiseNISRACaseProcessor_UnitTests
{
    [TestClass]
    public class DataLinkTests
    {
        [TestMethod]
        [DataRow(@"D:\Temp\Processor\32bit\OPN2004A.bdix")]
        [DataRow(@"D:\Temp\Processor\64bit\OPN2004A.bdix")]
        [DataRow(@"D:\Temp\Processor\test\OPN2004A.bdix")]
        public void Given_A_Bdix_File_When_I_Call_GetDataLink_A_Datalink_Is_Returned(string file)
        {
            //act
            var dataLink = DataLinkManager.GetDataLink(file);
            
            //assert
            Assert.IsNotNull(dataLink);
        }
    }
}
