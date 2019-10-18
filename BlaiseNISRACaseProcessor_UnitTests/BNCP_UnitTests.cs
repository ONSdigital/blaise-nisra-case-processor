using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlaiseNISRACaseProcessor_UnitTests
{
    [TestClass]
    public class BNCP_UnitTests
    {
        [TestMethod]
        public void Test_Get_BDI_File()
        {
            var fileName = BlaiseNISRACaseProcessor.BlaiseMethods.GetBDIFile("c:\\Blaise_NISRA_Drop", "OPN1911A");
            Assert.AreNotEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_BDI_File_Fail_Source()
        {
            var fileName = BlaiseNISRACaseProcessor.BlaiseMethods.GetBDIFile("c:\\Blaise_NISRA_Drop_FOOBAR", "OPN1911A");
            Assert.AreEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_BDI_File_Fail_Instrument()
        {
            var fileName = BlaiseNISRACaseProcessor.BlaiseMethods.GetBDIFile("c:\\Blaise_NISRA_Drop", "FOOBAR");
            Assert.AreEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name()
        {
            string dfName = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName("LocalDevelopment", "OPN1911A");
            Assert.AreNotEqual("", dfName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name_Fail_SP()
        {
            string dfName = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName("FOOBAR", "OPN1911A");
            Assert.AreEqual("", dfName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name_Fail_Instrument()
        {
            string dfName = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName("LocalDevelopment", "FOOBAR");
            Assert.AreEqual("", dfName);
        }
        
        [TestMethod]
        public void Test_Get_Connection()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");
            string binding = ConfigurationManager.AppSettings.Get("BlaiseServerBinding");
            var connection = BlaiseNISRACaseProcessor.BlaiseMethods.ConnectToBlaiseServer(serverName, username, password, binding);
            Assert.AreNotEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_Password()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = "FOOBAR";
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];
            var connection = BlaiseNISRACaseProcessor.BlaiseMethods.ConnectToBlaiseServer(serverName, username, password, binding);
            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_User()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = "FOOBAR";
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];
            var connection = BlaiseNISRACaseProcessor.BlaiseMethods.ConnectToBlaiseServer(serverName, username, password, binding);
            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_Server()
        {
            string serverName = "FOOBAR";
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];
            var connection = BlaiseNISRACaseProcessor.BlaiseMethods.ConnectToBlaiseServer(serverName, username, password, binding);
            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Password()
        {
            var securePassword = BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor.GetPassword("password");
            Assert.AreEqual(8, securePassword.Length);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI()
        {
            string serverPark = "LocalDevelopment";
            string instrument = "OPN1911A";
            string originalBDI = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName(serverPark, instrument);
            var originalDataLink = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataLinkFromBDI(originalBDI);
            Assert.AreNotEqual(null, originalDataLink);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI_Fail_SP()
        {
            string serverPark = "FOOBAR";
            string instrument = "OPN1911A";
            string originalBDI = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName(serverPark, instrument);
            var originalDataLink = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataLinkFromBDI(originalBDI);
            Assert.AreEqual(null, originalDataLink);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI_Fail_Instrument()
        {
            string serverPark = "LocalDevelopment";
            string instrument = "FOOBAR";
            string originalBDI = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataFileName(serverPark, instrument);
            var originalDataLink = BlaiseNISRACaseProcessor.BlaiseMethods.GetDataLinkFromBDI(originalBDI);
            Assert.AreEqual(null, originalDataLink);
        }
    }
}
