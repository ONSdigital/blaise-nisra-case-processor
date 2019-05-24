using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.ServerManager;
using System.Configuration;
using System.Timers;

namespace BlaiseNISRACaseProcessor
{
    public partial class BlaiseNISRACaseProcessor: ServiceBase
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BlaiseNISRACaseProcessor()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            this.Run();
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Blaise NISRA Case Processor service started.");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.ProcessFiles);
            timer.Start();
        }

        protected override void OnStop()
        {
            log.Info("Blaise NISRA Case Processor service stopped.");
        }

        private void ProcessFiles(object sender, ElapsedEventArgs args)
        {
            Run();
        }

        public bool Run()
        {
            // Connection parameters
            string serverName = ConfigurationManager.AppSettings["BlaiseServerHostName"];
            string userName = ConfigurationManager.AppSettings["BlaiseServerUserName"];
            string password = ConfigurationManager.AppSettings["BlaiseServerPassword"];

            IConnectedServer serverManagerConnection = null;
            try
            {
                log.Info("Attempting to connect to Blaise Server Manager.");
                serverManagerConnection = ServerManager.ConnectToServer(serverName, 8031, userName, GetPassword(password));
            }
            catch (Exception e)
            {
                log.Error("Error connecting to Blaise Server Manager.");
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return false;
            }

            // Loop through the server parks on the connected Blaise server.
            foreach (IServerPark serverPark in serverManagerConnection.ServerParks)
            {
                // Loop through the surveys installed on the current server park
                foreach (ISurvey survey in serverManagerConnection.GetServerPark(serverPark.Name).Surveys)
                {
                    ProcessSurvey(serverPark.Name, survey.Name);
                }
            }
            return true;
        }

        public bool ProcessSurvey(string serverPark, string instrument)
        {
            try
            {
                log.Info(String.Format("Processing - {0}/{1}", serverPark, instrument));

                var dataDropFolder = "c:\\NISRAData_VPN";

                string nisraBDI = GetBDIFile(dataDropFolder, instrument);

                if (nisraBDI != "")
                {
                    log.Info(String.Format("NISRA .bdi file found. - {0}", nisraBDI));

                    // Get the BDI files for the survey stored on the Blaise server:
                    string blaiseServerBDI = GetDataFileName(serverPark, instrument);

                    // Get data links for the nisra file and the blaise server data interfaces:
                    var nisraFileDataLink = GetDataLinkFromBDI(nisraBDI);
                    var blaiseServerDataLink = GetDataLinkFromBDI(blaiseServerBDI);

                    if (nisraFileDataLink == null || blaiseServerDataLink == null)
                        return false;
                    else
                    {
                        ImportDataRecords(nisraFileDataLink, blaiseServerDataLink);
                        return true;
                    }
                }
                else
                {
                    log.Info(String.Format("No NISRA file found for: {0}/{1}.", serverPark, instrument));
                    return false;
                }
            }
            catch (Exception e)
            {
                log.Error(String.Format("Error Processing survey: {0}/{1}",serverPark,instrument));
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Searches for a .bdi file matching the instrument name in the source directory provided.
        /// </summary>
        /// <param name="sourceDirectory"> The directory where the target BDI file exists. </param>
        /// <param name="instrument"> The name of the instrument (i.e OPN1901A). </param>
        /// <returns> String representation of the file path. </returns>
        public string GetBDIFile(string sourceDirectory, string instrument = "")
        {
            try
            {
                List<string> ext = new List<string> { ".bdi", ".bdix" };
                var bdiFiles = Directory.GetFiles(sourceDirectory, String.Format("*{0}.*", instrument), SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s)));
                if (bdiFiles.Count() > 0)
                    return bdiFiles.ElementAt(0);
                else
                    return "";
            }
            catch (Exception e)
            {
                log.Error(String.Format("Error Getting BDI file: {0}/{1}", sourceDirectory, instrument));
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// Establishes a connection to a Blaise Server.
        /// </summary>
        /// <param name="serverName">The location of the Blaise server.</param>
        /// <param name="userName">Username with access to the specified server.</param>
        /// <param name="password">Password for the specified user to access the server.</param>
        /// <returns>A IConnectedServer2 object which is connected to the server provided.</returns>
        public IConnectedServer2 ConnectToBlaiseServer(string serverName, string userName, string password)
        {
            int port = 8031;
            try
            {
                IConnectedServer2 connServer =
                    (IConnectedServer2)ServerManager.ConnectToServer(serverName, port, userName, GetPassword(password));

                return connServer;
            }
            catch (Exception e)
            {
                log.Error("Error getting Blaise connection.");
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Converts a string password to the SecureString format.
        /// </summary>
        /// <param name="pw">The string password being read in for conversion.</param>
        /// <returns>A SecureString version of the imported password.</returns>
        public static SecureString GetPassword(string pw)
        {
            char[] passwordChars = pw.ToCharArray();
            SecureString password = new SecureString();
            foreach (char c in passwordChars)
            {
                password.AppendChar(c);
            }
            return password;
        }

        /// <summary>
        /// Gets the name of the data file (.bdix) associated with a specified serverpark and instrument.
        /// </summary>
        /// <param name="serverPark">The serverpark where the instrument exists.</param>
        /// <param name="instrument">The instrument who's data file we're getting.</param>
        /// <returns>The string name of the data file (.bdix)</returns>
        public string GetDataFileName(string serverPark, string instrument)
        {
            try
            {
                string serverName = ConfigurationManager.AppSettings.Get("BlaiseServerHostName");
                string username = ConfigurationManager.AppSettings.Get("BlaiseServerUserName");
                string password = ConfigurationManager.AppSettings.Get("BlaiseServerPassword");

                var connection = ConnectToBlaiseServer(serverName, username, password);


                var surveys = connection.GetSurveys(serverPark);

                foreach (ISurvey2 survey in surveys)
                {
                    if (survey.Name == instrument)
                    {
                        var conf = survey.Configuration.Configurations.ElementAt(0);

                        return conf.DataFileName;
                    }
                }

                return "";
            }
            catch (Exception e)
            {
                log.Error("Error getting meta file name.");
                log.Error(e.Message);
                log.Error(e.StackTrace);

                return "";
            }
        }

        /// <summary>
        /// Gets and returns a datalink connection to the target data file (.bdix).
        /// </summary>
        /// <param name="bdiFile">The name of the target data file (.bdix).</param>
        /// <returns>A IDatalink connection object user to access the stored Blaise data.</returns>
        public IDataLink GetDataLinkFromBDI(string bdiFile)
        {
            try
            {
                var dl = DataLinkManager.GetDataLink(bdiFile);
                return dl;
            }
            catch (Exception e)
            {
                log.Error("Error Getting DataLink");
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Uses two datalink objects to import the data from one source to another.
        /// </summary>
        /// <param name="sourceDL">A datalink object referencing the source data location.</param>
        /// <param name="targetDL">A datalink object referencing the target data location.</param>
        public bool ImportDataRecords(IDataLink sourceDL, IDataLink targetDL)
        {
            // This function will need to consider the rules surrounding NISRA data import.
            // Below this return statement is the code required to copy records directly to the database.
            return true;
            try
            {
                IDataSet ds = sourceDL.Read("");

                while (!ds.EndOfSet)
                {
                    // Read the current record and write it to the backup database:
                    var dr = ds.ActiveRecord;
                    targetDL.Write(dr);

                    // Move to the next record:
                    ds.MoveNext();
                }
            }
            catch (Exception e)
            {
                log.Error("Error Importing data records.");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
        }
    }
}
