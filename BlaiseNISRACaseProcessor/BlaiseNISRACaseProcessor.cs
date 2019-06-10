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
using StatNeth.Blaise.API.DataRecord;
using StatNeth.Blaise.API.Meta;
using System.Globalization;

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
            //EditNisraData();
            //return;
            this.Run();
        }

        public void EditNisraData()
        {
            var dataDropFolder = ConfigurationManager.AppSettings["NisraDataFolder"];
            string nisraBDI = GetBDIFile(dataDropFolder, "OPN1901A");

            var nisraFileDataLink = GetDataLinkFromBDI(nisraBDI);


            // Read the NISRA data into a dataset object.
            IDataSet nisraDataset = nisraFileDataLink.Read("");

            string[] values = { "110", "200", "300" };

            int count = 0;
            // Loop through every record within the NISRA dataset
            while (!nisraDataset.EndOfSet && (count < values.Length))
            {
                // Read the current record.
                var nisraRecord = nisraDataset.ActiveRecord;


                // Get the completed and processed flags from the current record
                var houtVal = nisraRecord.GetField("QAdmin.Hout");
                houtVal.DataValue.Assign(values[count]);

                nisraFileDataLink.Write(nisraRecord);

                count++;
                nisraDataset.MoveNext();
            }
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Blaise NISRA Case Processor service started.");            
            
            // Get the MinuteRunTimer env variable and convert it from minutes to miliseconds.
            string timerString = ConfigurationManager.AppSettings["MinuteRunTimer"];
            double time = double.Parse(timerString, CultureInfo.InvariantCulture.NumberFormat);
            time = time * 60 * 1000;

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = time; 
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
                    ProcessSurvey(serverPark, survey);
                }
            }
            return true;
        }

        public bool ProcessSurvey(IServerPark serverPark, ISurvey instrument)
        {
            try
            {
                log.Info(String.Format("Processing - {0}/{1}", serverPark.Name, instrument.Name));

                var dataDropFolder = ConfigurationManager.AppSettings["NisraDataFolder"];

                string nisraBDI = GetBDIFile(dataDropFolder, instrument.Name);

                if (nisraBDI != "")
                {
                    log.Info(String.Format("NISRA .bdi file found. - {0}", nisraBDI));
                    
                    // Get data links for the nisra file and the blaise server data interfaces:
                    var nisraFileDataLink = GetDataLinkFromBDI(nisraBDI);
                    var blaiseServerDataLink = GetRemoteDataLink(serverPark, instrument);

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
                    log.Info(String.Format("No NISRA file found for: {0}/{1}.", serverPark.Name, instrument.Name));
                    return false;
                }
            }
            catch (Exception e)
            {
                log.Error(String.Format("Error Processing survey: {0}/{1}",serverPark.Name,instrument.Name));
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
        /// Method for connecting to Blaise data sets.
        /// </summary>
        /// /// <param name="hostname">The name of the hostname.</param>
        /// <param name="instrumentName">The name of the instrument.</param>
        /// <param name="serverPark">The name of the server park.</param>
        /// <returns> IDataLink4 object for the connected server park.</returns>
        public static IDataLink4 GetRemoteDataLink(IServerPark serverPark, ISurvey instrument)
        {
            string serverName = ConfigurationManager.AppSettings["BlaiseServerHostName"];
            string userName = ConfigurationManager.AppSettings["BlaiseServerUserName"];
            string password = ConfigurationManager.AppSettings["BlaiseServerPassword"];

            // Get the GIID of the instrument.
            Guid instrumentID = Guid.NewGuid();
            try
            {
                instrumentID = instrument.InstrumentID;

                // Connect to the data.
                IRemoteDataServer dataLinkConn = DataLinkManager.GetRemoteDataServer(serverName, 8033, userName, GetPassword(password));

                return dataLinkConn.GetDataLink(instrumentID, serverPark.Name);
            }
            catch (Exception e)
            {
                log.Error("Error connecting to remote data link.");
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
        public bool ImportDataRecords(IDataLink nisraDatalink, IDataLink4 dbDatalink)
        {
            // This function will need to consider the rules surrounding NISRA data import.
            // Below this return statement is the code required to copy records directly to the database.
            //return true;
            try
            {
                // Read the NISRA data into a dataset object.
                IDataSet nisraDataset = nisraDatalink.Read("");

                // Loop through every record within the NISRA dataset
                while (!nisraDataset.EndOfSet)
                {
                    // Read the current record.
                    var nisraRecord = nisraDataset.ActiveRecord;

                    // Get the key from the NISRA datalink's attached data model.
                    IDatamodel sourceModel = nisraRecord.Datamodel;
                    var key = DataRecordManager.GetKey(sourceModel, "PRIMARY");
                    
                    // Assign the serial number of the current nisra data record to the key value.
                    string serialNumber = nisraRecord.Keys[0].KeyValue;
                    key.Fields[0].DataValue.Assign(serialNumber);
                    
                    // Check if a case with this key exists in the source data set. (this will find our matching record)
                    if (dbDatalink.KeyExists(key))
                    {
                        // If it does then get the record using the generated key
                        var dbRecord = dbDatalink.ReadRecord(key);

                        // Get both of the record's Hout field (NISRA & the database) 
                        var nisraHout = nisraRecord.GetField("QAdmin.Hout");
                        var dbHout = dbRecord.GetField("QAdmin.Hout");

                        if (!(nisraHout.DataValue.IntegerValue == 0))
                        {
                            // Compare the two outcome values. If the nisra value is lower then the stored value then replace it.
                            if (nisraHout.DataValue.IntegerValue < dbHout.DataValue.IntegerValue || dbHout.DataValue.IntegerValue == 0)
                            {
                                log.Info(String.Format("Serial {0} - Nisra case has a better outcome. Nisra: {1} / DB: {2}", serialNumber.Trim(' '), nisraHout.DataValue.IntegerValue, dbHout.DataValue.IntegerValue));
                                dbDatalink.Write(nisraRecord);
                            }
                            else
                            {
                                log.Info(String.Format("Serial {0} - Database case has a better or equal outcome. Nisra: {1} / DB: {2}", serialNumber.Trim(' '), nisraHout.DataValue.IntegerValue, dbHout.DataValue.IntegerValue));
                            }
                        }
                        else
                        {
                            log.Info(String.Format("Serial {0} - Nisra case has not been processed. Nisra: {1} / DB: {2}", serialNumber.Trim(' '), nisraHout.DataValue.IntegerValue, dbHout.DataValue.IntegerValue));
                        }
                    }

                    // Move to the next record:
                    nisraDataset.MoveNext();
                }
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error Importing data records.");
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return false;
            }
        }
    }
}
