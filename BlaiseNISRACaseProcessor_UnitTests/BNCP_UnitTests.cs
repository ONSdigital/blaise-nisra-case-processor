using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlaiseNISRACaseProcessor_UnitTests
{
    [TestClass]
    public class BNCP_UnitTests
    {
        [TestMethod]
        public void Test_Run()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            bool success = b.Run();

            Assert.AreEqual(true, success);
        }

        //[TestMethod]
        //public void Test_ProcessSurvey()
        //{
        //    var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

        //    bool success = b.ProcessSurvey("LocalDevelopment","OPN1901A");

        //    Assert.AreEqual(true, success);
        //}

        [TestMethod]
        public void Test_Get_BDI_File()
        {
            var dataDropFolder = "c:\\NISRAData_VPN";

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string fileName = b.GetBDIFile(dataDropFolder, "OPN1901A");

            Assert.AreNotEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_BDI_File_Fail_Source()
        {
            var badDataFolder = "c:\\NISRAData_VPN_Bad";

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string fileName = b.GetBDIFile(badDataFolder, "OPN1901A");

            Assert.AreEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_BDI_File_Fail_Instrument()
        {
            var dataDropFolder = "c:\\NISRAData_VPN";

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string fileName = b.GetBDIFile(dataDropFolder, "OPN1901A_BAD");

            Assert.AreEqual("", fileName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            string dfName = b.GetDataFileName("LocalDevelopment", "OPN1901A");

            Assert.AreNotEqual("", dfName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name_Fail_SP()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            string dfName = b.GetDataFileName("BadServerPark", "OPN1901A");

            Assert.AreEqual("", dfName);
        }

        [TestMethod]
        public void Test_Get_Data_File_Name_Fail_Instrument()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            string dfName = b.GetDataFileName("LocalDevelopment", "BadInstrument");

            Assert.AreEqual("", dfName);
        }


        [TestMethod]
        public void Test_Get_Connection()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            var connection = b.ConnectToBlaiseServer(serverName, username, password);

            Assert.AreNotEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_Password()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = "BadPassword";

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            var connection = b.ConnectToBlaiseServer(serverName, username, password);

            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_User()
        {
            string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
            string username = "BadUser";
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            var connection = b.ConnectToBlaiseServer(serverName, username, password);

            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Connection_Fail_Server()
        {
            string serverName = "BadServer";
            string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
            string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");

            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();
            var connection = b.ConnectToBlaiseServer(serverName, username, password);

            Assert.AreEqual(null, connection);
        }

        [TestMethod]
        public void Test_Get_Password()
        {
            string password = "Password";

            var securePassword = BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor.GetPassword(password);

            Assert.AreEqual(8, securePassword.Length);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string serverPark = "LocalDevelopment";
            string instrument = "HealthSurvey";
            // Get the BMI and BDI files for the survey:
            string originalBDI = b.GetDataFileName(serverPark, instrument);


            // Get data links for the original and the backup data interfaces:
            var originalDataLink = b.GetDataLinkFromBDI(originalBDI);

            Assert.AreNotEqual(null, originalDataLink);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI_Fail_SP()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string serverPark = "BadServerpark";
            string instrument = "HealthSurvey";
            // Get the BMI and BDI files for the survey:
            string originalBDI = b.GetDataFileName(serverPark, instrument);


            // Get data links for the original and the backup data interfaces:
            var originalDataLink = b.GetDataLinkFromBDI(originalBDI);

            Assert.AreEqual(null, originalDataLink);
        }

        [TestMethod]
        public void Test_Get_Datalink_From_BDI_Fail_Instrument()
        {
            var b = new BlaiseNISRACaseProcessor.BlaiseNISRACaseProcessor();

            string serverPark = "LocalDevelopment";
            string instrument = "BadInstrument";
            // Get the BMI and BDI files for the survey:
            string originalBDI = b.GetDataFileName(serverPark, instrument);


            // Get data links for the original and the backup data interfaces:
            var originalDataLink = b.GetDataLinkFromBDI(originalBDI);

            Assert.AreEqual(null, originalDataLink);
        }
    }
}
