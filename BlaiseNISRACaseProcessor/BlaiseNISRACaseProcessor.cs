using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.ServiceProcess;
using System.Text;
using RabbitMQ.Client;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.ServerManager;
using System.Configuration;
using System.Timers;
using StatNeth.Blaise.API.DataRecord;
using StatNeth.Blaise.API.Meta;
using System.Globalization;
using System.Web.Script.Serialization;

namespace BlaiseNISRACaseProcessor
{
    public partial class BlaiseNISRACaseProcessor : ServiceBase
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Objects for RabbitMQ.
        public IConnection connection;
        public IModel channel;

        public BlaiseNISRACaseProcessor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method for creating synthetic HOUT data for testing.
        /// </summary>
        public void EditNisraData()
        {

            var nistaProcessFolder = ConfigurationManager.AppSettings["NisraProcessFolder"];
            string nisraBDI = GetBDIFile(nistaProcessFolder, "OPN1901A");
            var nisraDataLink = GetDataLinkFromBDI(nisraBDI);
            IDataSet nisraDataset = nisraDataLink.Read("");
            string[] values = { "110","110", "110", "110" };
            int count = 0;
            while (!nisraDataset.EndOfSet && (count < values.Length))
            {
                var nisraRecord = nisraDataset.ActiveRecord;
                
                var houtVal = nisraRecord.GetField("QHAdmin.Hout");
                houtVal.DataValue.Assign(values[count]);

                var whoMadeValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[1].WhoMade");
                whoMadeValue.DataValue.Assign("NISRA");

                var dayNumberValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[1].DayNumber");
                dayNumberValue.DataValue.Assign("1");
                
                var nrOfDialsValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[1].NrOfDials");
                nrOfDialsValue.DataValue.Assign("1");

                var callOutcomeValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[1].DialResult");
                callOutcomeValue.DataValue.Assign("1");

                whoMadeValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[5].WhoMade");
                whoMadeValue.DataValue.Assign("NISRA");

                dayNumberValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[5].DayNumber");
                dayNumberValue.DataValue.Assign("1");

                nrOfDialsValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[5].NrOfDials");
                nrOfDialsValue.DataValue.Assign("1");

                callOutcomeValue = nisraRecord.GetField("CatiMana.CatiCall.RegsCalls[5].DialResult");
                callOutcomeValue.DataValue.Assign("1");

                var completedVal = nisraRecord.GetField("Completed");
                completedVal.DataValue.Assign("1");
                
                var excludeVal = nisraRecord.GetField("Exclude");
                excludeVal.DataValue.Assign("1");

                var webStatusVal = nisraRecord.GetField("WebFormStatus");
                webStatusVal.DataValue.Assign("1");

                nisraDataLink.Write(nisraRecord);
                count++;
                nisraDataset.MoveNext();
            }
        }

        public void OnDebug()
        {
            //EditNisraData();
            this.Run();
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
            timer.Elapsed += new ElapsedEventHandler(this.TimerRun);
            timer.Start();
        }

        protected override void OnStop()
        {
            log.Info("Blaise NISRA Case Processor service stopped.");
        }

        private void TimerRun(object sender, ElapsedEventArgs args)
        {
            Run();
        }

        public void Run()
        {
            // Get NISRA processing folder from app config.
            string nisraProcessFolder = ConfigurationManager.AppSettings["NisraProcessFolder"];

            // Get Blaise server details from app config.
            string serverName = ConfigurationManager.AppSettings["BlaiseServerHostName"];
            string userName = ConfigurationManager.AppSettings["BlaiseServerUserName"];
            string password = ConfigurationManager.AppSettings["BlaiseServerPassword"];
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];

            // Look for BDIX files in the NISRA processing folder.
            string[] bdixFiles = Directory.GetFiles(nisraProcessFolder, "*.bdix", SearchOption.TopDirectoryOnly);

            // If a BDIX file is found, process it.
            if (bdixFiles.Any())
            {
                // Process all the BDIX files found.
                foreach (var bdixFile in bdixFiles)
                {
                    log.Info("Processing NISRA file - " + bdixFile);
                    // Connect to the Blaise server.
                    IConnectedServer serverManagerConnection = null;
                    try
                    {
                        log.Info("Connecting to Blaise server - " + serverName);
                        serverManagerConnection = ServerManager.ConnectToServer(serverName, 8031, userName, GetPassword(password), binding);
                    }
                    catch (Exception e)
                    {
                        log.Error("Error connecting to Blaise server - " + serverName);
                        log.Error(e.Message);
                        log.Error(e.StackTrace);
                    }
                    // Loop through the server parks on the connected Blaise server.
                    foreach (IServerPark serverPark in serverManagerConnection.ServerParks)
                    {
                        // Loop through the surveys installed on the current server park
                        foreach (ISurvey survey in serverManagerConnection.GetServerPark(serverPark.Name).Surveys)
                        {
                            // If a survey is found that matches the NISRA file, process it.
                            if (survey.Name == Path.GetFileNameWithoutExtension(bdixFile))
                            {
                                log.Info("Survey found on server (" + serverPark.Name + "/" + survey.Name + ") that matches NISRA file.");
                                ProcessSurvey(serverPark, survey);
                            }
                            else
                            {
                                log.Warn("No survey found on Blaise server that matches NISRA file.");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process the survey data.
        /// </summary>
        public void ProcessSurvey(IServerPark serverPark, ISurvey instrument)
        {
            try
            {
                log.Info($"Starting ProcessSurvey for ServerPark: {serverPark.Name}, Instrument: {instrument.Name}");
                // Get process and backup folder locations from app config.
                var nisraProcessFolder = ConfigurationManager.AppSettings["NisraProcessFolder"];
                var nisraBackupFolder = ConfigurationManager.AppSettings["NisraBackupFolder"];
                log.Debug($"NISRAProcessFolder: {nisraProcessFolder}, NISRABackupFolder: {nisraBackupFolder}");
                // Get path for NISRA BDIX.
                string nisraBDI = GetBDIFile(nisraProcessFolder, instrument.Name);
                // Get data links for the NISRA file and Blaise server.
                var nisraFileDataLink = GetDataLinkFromBDI(nisraBDI);
                var blaiseServerDataLink = GetRemoteDataLink(serverPark, instrument);
                // If we have data links for the NISRA file and the Blaise server.
                if (nisraFileDataLink != null || blaiseServerDataLink != null)
                {
                    // Attempt to import the cases if necessary.
                    if (ImportDataRecords(nisraFileDataLink, blaiseServerDataLink, instrument, serverPark))
                    {
                        // Move the NISRA file to backup location if it's been sucessfully processed.
                        MoveFiles(nisraProcessFolder, nisraBackupFolder, MoveType.Move, instrument.Name);
                    }
                }
                else
                {
                    log.Warn("Could not get data links for NISRA file and/or Blaise server.");
                }
            }
            catch (Exception e)
            {
                log.Error($"Error Processing survey, ServerPark: {serverPark.Name}, InstrumentName{instrument.Name}");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
        }

        /// <summary>
        /// Uses two datalink objects to import the data from one source to another.
        /// </summary>
        /// <param name="sourceDL">A datalink object referencing the source data location.</param>
        /// <param name="targetDL">A datalink object referencing the target data location.</param>
        public bool ImportDataRecords(IDataLink nisraDatalink, IDataLink4 serverDataLink, ISurvey instrument, IServerPark serverPark)
        {
            try
            {
                // Read the NISRA data into a dataset object.
                IDataSet nisraDataSet = nisraDatalink.Read("");
                // Connect to Rabbit.
                SetupRabbit();
                // Loop through every record within the NISRA dataset.
                log.Info("Looping through NISRA data.");
                while (!nisraDataSet.EndOfSet)
                {
                    // Read the current record/case.
                    var nisraRecord = nisraDataSet.ActiveRecord;
                    // Get the key field from the model.
                    IDatamodel sourceModel = nisraRecord.Datamodel;
                    var key = DataRecordManager.GetKey(sourceModel, "PRIMARY");
                    // Get the value of the key field.
                    string serialNumber = nisraRecord.Keys[0].KeyValue;
                    key.Fields[0].DataValue.Assign(serialNumber);
                    // Check if a record/case with the key field value exists on the Blaise server.
                    if (serverDataLink.KeyExists(key))
                    {
                        // Get the record/case on the server.
                        var serverRecord = serverDataLink.ReadRecord(key);

                        // Check if the WebFormStatus field exists in the NISRA and server record
                        if (DataRecordExtensions.CheckForField(nisraRecord, "WebFormStatus") && DataRecordExtensions.CheckForField(serverRecord, "WebFormStatus"))
                        {
                            var serverStatus = serverRecord.GetField("WebFormStatus");
                            var nisraStatus = nisraRecord.GetField("WebFormStatus");

                            // if the nisra status value is null it has not been processed so move to the next record (0 = Null)
                            if (nisraStatus.DataValue.EnumerationValue == 0)
                            {
                                nisraDataSet.MoveNext();
                                continue;
                            }

                            // If the server status value is "Complete" and the NISRA status is also "Complete" then process the record's Hout values. (1 = Complete, 2 = Partial)  
                            // Otherwise do nothing and move to the next record.
                            if (serverStatus.DataValue.EnumerationValue == 1)
                            {
                                if (nisraStatus.DataValue.EnumerationValue == 1)
                                {
                                    ProcessRecordHoutValues(nisraRecord, serverRecord, serverDataLink, instrument, serverPark, serialNumber);
                                }
                                else
                                {
                                    nisraDataSet.MoveNext();
                                    continue;
                                }
                            }
                            // If the server status value is "Partial" and the NISRA status is also "Partial" then process the record's Hout values. (1 = Complete, 2 = Partial)  
                            // Otherwise write the completed NISRA case to the server.
                            else if (serverStatus.DataValue.EnumerationValue == 2)
                            {
                                if (nisraStatus.DataValue.EnumerationValue == 2)
                                {
                                    // Compare HOUT values
                                    ProcessRecordHoutValues(nisraRecord, serverRecord, serverDataLink, instrument, serverPark, serialNumber);
                                }
                                else
                                {
                                    // Replace server record with NISRA record
                                    log.Info(String.Format("Serial {0} - NISRA has better WebFormStatus (NISRA: {1} / Server: {2}). Updating server.", serialNumber.Trim(' '), nisraStatus.DataValue.EnumerationValue, serverStatus.DataValue.EnumerationValue));

                                    //Replace Server record with NISRA
                                    WriteNisraRecordToServer(nisraRecord, serverRecord, serverDataLink);

                                    // Make the JSON status update message.
                                    var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA Case Imported");

                                    // Send the JSON status update message.
                                    SendStatus(json);
                                }
                            }
                            // If the nisraStatus value is Partial or Complete write it over the untouched server record.
                            else
                            {
                                if (nisraStatus.DataValue.EnumerationValue > 0)
                                {
                                    // Replace server record with NISRA record
                                    log.Info(String.Format("Serial {0} - NISRA has better WebFormStatus (NISRA: {1} / Server: {2}). Updating server.", serialNumber.Trim(' '), nisraStatus.DataValue.EnumerationValue, serverStatus.DataValue.EnumerationValue));

                                    //Replace Server record with NISRA
                                    WriteNisraRecordToServer(nisraRecord, serverRecord, serverDataLink);

                                    // Make the JSON status update message.
                                    var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA Case Imported");

                                    // Send the JSON status update message.
                                    SendStatus(json);
                                }
                            }
                        }
                        else
                        {
                            // If there's no WebFormStatus field, attempt to process the record using the Hout values
                            ProcessRecordHoutValues(nisraRecord, serverRecord, serverDataLink, instrument, serverPark, serialNumber);
                        }
                    }
                    else
                    {
                        // If no case if found, write the record stright to the Blaise server (IPS behaviour).
                        serverDataLink.Write(nisraRecord);
                    }
                    // Move to the next record:
                    nisraDataSet.MoveNext();
                }
                connection.Close();
                return true;
            }
            catch (Exception e)
            {
                log.Error("Error importing data records.");
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return false;
            }
        }

        public void ProcessRecordHoutValues(StatNeth.Blaise.API.DataRecord.IDataRecord nisraRecord, StatNeth.Blaise.API.DataRecord.IDataRecord serverRecord, IDataLink4 serverDataLink, 
                                            ISurvey instrument, IServerPark serverPark, string serialNumber)
        {
            // Check for an HOUT field in the NISRA data.
            if (DataRecordExtensions.CheckForField(nisraRecord, "QHAdmin.HOut"))
            {
                // Get the NISTA HOUT.
                var nisraHOUT = nisraRecord.GetField("QHAdmin.HOut");
                log.Info("NISRA record: " + nisraRecord + " HOut: " + nisraHOUT);
                // If HOUT is not 0.
                if (!(nisraHOUT.DataValue.IntegerValue == 0))
                {
                    // Get the HOUT of the record/case on the server.
                    var serverHOUT = serverRecord.GetField("QHAdmin.HOut");
                    // Compare the HOUT of the record/case in the NISRA file and on the server.
                    // Write the NISRA record/case to the server if it's HOUT is lower.
                    if (nisraHOUT.DataValue.IntegerValue < serverHOUT.DataValue.IntegerValue || serverHOUT.DataValue.IntegerValue == 0)
                    {
                        log.Info(String.Format("Serial {0} - NISRA has better HOUT (NISRA: {1} / Server: {2}). Updating server.", serialNumber.Trim(' '), nisraHOUT.DataValue.IntegerValue, serverHOUT.DataValue.IntegerValue));

                        //Replace Server record with NISRA
                        WriteNisraRecordToServer(nisraRecord, serverRecord, serverDataLink);

                        // Make the JSON status update message.
                        var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA Case Imported");

                        // Send the JSON status update message.
                        SendStatus(json);
                    }
                    else
                    {
                        log.Info(String.Format("Serial {0} - Server has better or equal HOUT (NISRA: {1} / Server: {2}).", serialNumber.Trim(' '), nisraHOUT.DataValue.IntegerValue, serverHOUT.DataValue.IntegerValue));
                    }
                }
                else
                {
                    log.Info(String.Format("Serial {0} - NISRA has not been processed (NISRA: {1}).", serialNumber.Trim(' '), nisraHOUT.DataValue.IntegerValue));
                }
            }
        }

        public void WriteNisraRecordToServer(StatNeth.Blaise.API.DataRecord.IDataRecord nisraRecord, StatNeth.Blaise.API.DataRecord.IDataRecord serverRecord, IDataLink4 serverDataLink)
        {
            // Get the Case_ID field objects for NISRA and the server.
            var serverCaseID = serverRecord.GetField("QID.Case_ID");
            var nisraCaseID = nisraRecord.GetField("QID.Case_ID");

            // Before we update the server record/case, take the Case_ID from the server data and put it in the NISRA data.
            // This is so that if the NISRA data doesn't have the Case_ID, it's not lost.
            nisraCaseID.DataValue.Assign(serverCaseID.DataValue.ValueAsText);

            // Modify the Online flag to indicate the new record is from the NISRA data set
            nisraRecord = DataRecordExtensions.AssignValueIfFieldExists(nisraRecord, "QHAdmin.Online", "1");

            // Update the server data with the NISRA data.
            serverDataLink.Write(nisraRecord);
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
                log.Error(String.Format("Error Getting BDI file: {0}{1}", sourceDirectory, instrument));
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
        public IConnectedServer2 ConnectToBlaiseServer(string serverName, string userName, string password, string binding)
        {
            int port = 8031;
            try
            {
                IConnectedServer2 connServer =
                    (IConnectedServer2)ServerManager.ConnectToServer(serverName, port, userName, GetPassword(password), binding);

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
                string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];

                var connection = ConnectToBlaiseServer(serverName, username, password, binding);

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
                log.Error("Error getting data link - " + bdiFile);
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
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];
            // Get the GIID of the instrument.
            Guid instrumentID = Guid.NewGuid();
            try
            {
                instrumentID = instrument.InstrumentID;
                // Connect to the data.
                IRemoteDataServer dataLinkConn = DataLinkManager.GetRemoteDataServer(serverName, 8033, binding, userName, GetPassword(password));
                return dataLinkConn.GetDataLink(instrumentID, serverPark.Name);
            }
            catch (Exception e)
            {
                log.Error("Error getting data link - " + serverPark.Name + "/" + instrumentID);
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
        /// Enumeration of Move or Copy for the MoveFiles method.
        /// </summary>
        public enum MoveType
        {
            Move,
            Copy
        }

        /// <summary>
        /// Move or copy files from path to path.
        /// </summary>
        public bool MoveFiles(string sourcePath, string targetPath, MoveType moveType, string instrumentName)
        {
            try
            {
                if (Directory.Exists(sourcePath))
                {
                    string[] files = Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string file in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        string fileName = System.IO.Path.GetFileName(file);

                        if (fileName.Contains(instrumentName))
                        {

                            string destFile = System.IO.Path.Combine(targetPath, fileName);

                            Directory.CreateDirectory(targetPath);
                            switch (moveType)
                            {
                                case MoveType.Move:
                                    System.IO.File.Delete(destFile);
                                    System.IO.File.Move(file, destFile);
                                    log.Info(String.Format("Successfully moved files from: {0} -> {1} ({2})", sourcePath, targetPath, fileName));
                                    break;
                                case MoveType.Copy:
                                    System.IO.File.Copy(file, destFile, true);
                                    log.Info(String.Format("Successfully copied files from: {0} -> {1} ({2})", sourcePath, targetPath, fileName));
                                    break;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    log.Error(String.Format("Unable to copy. Source path doesnt exist: {0}", sourcePath));
                    return false;
                }
            }
            catch (Exception e)
            {
                log.Error(String.Format("Error moving files: {0} -> {1}", sourcePath, targetPath));
                log.Error(e.Message);
                log.Error(e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method for connecting to RabbitMQ and setting up the channels.
        /// </summary>
        public bool SetupRabbit()
        {
            log.Info("Setting up RabbitMQ.");
            try
            {
                // Create a connection to RabbitMQ using the Rabbit credentials stored in the app.config file.
                var connFactory = new ConnectionFactory()
                {
                    HostName = ConfigurationManager.AppSettings["RabbitHostName"],
                    UserName = ConfigurationManager.AppSettings["RabbitUserName"],
                    Password = ConfigurationManager.AppSettings["RabbitPassword"]
                };
                connection = connFactory.CreateConnection();
                channel = connection.CreateModel();
                // Get the exchange and queue details from the app.config file.
                string exchangeName = ConfigurationManager.AppSettings["RabbitExchange"];
                string queueName = ConfigurationManager.AppSettings["CaseStatusQueueName"];
                // Declare the exchange for sending messages.
                channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: true);
                log.Info("Exchange declared - " + exchangeName);
                // Declare the queue for sending message updates.
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                log.Info("Queue declared - " + queueName);
                log.Info("RabbitMQ setup complete.");
                return true;
            }
            catch
            {
                log.Info("Unable to establish RabbitMQ connection.");
                return false;
            }
        }

        /// <summary>
        /// Builds a JSON status object to be used in the SendStatus method.
        /// </summary>
        public Dictionary<string, string> MakeJsonStatus(StatNeth.Blaise.API.DataRecord.IDataRecord recordData, string instrumentName, string serverPark, string status)
        {
            Dictionary<string, string> jsonData = new Dictionary<string, string>();
            if (recordData != null)
            {
                foreach (IField qidField in recordData.GetField("QID").Fields)
                {
                    jsonData[qidField.LocalName] = qidField.DataValue.ValueAsText.ToLower();
                }
            }
            jsonData["instrument_name"] = instrumentName;
            jsonData["server_park"] = serverPark;
            jsonData["status"] = status;
            return jsonData;
        }

        /// <summary>
        /// Sends a status message to RabbitMQ.
        /// </summary>
        private void SendStatus(Dictionary<string, string> jsonData)
        {
            string message = new JavaScriptSerializer().Serialize(jsonData);
            var body = Encoding.UTF8.GetBytes(message);
            string caseStatusQueueName = ConfigurationManager.AppSettings["CaseStatusQueueName"];
            channel.BasicPublish(exchange: "", routingKey: caseStatusQueueName, body: body);
            log.Info("Message sent to RabbitMQ " + caseStatusQueueName + " queue - " + message);
        }

        
    }
}
