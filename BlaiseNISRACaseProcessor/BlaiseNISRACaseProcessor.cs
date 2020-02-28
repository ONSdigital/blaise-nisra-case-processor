using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.ServiceProcess;
using System.Text;
using RabbitMQ.Client;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.ServerManager;
using StatNeth.Blaise.API.DataRecord;
using StatNeth.Blaise.API.Meta;
using System.Configuration;
using System.Web.Script.Serialization;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace BlaiseNISRACaseProcessor
{
    public partial class BlaiseNISRACaseProcessor : ServiceBase
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Instantiate scheduler.
        private static IScheduler _scheduler;

        // Objects for RabbitMQ.
        public static IConnection connection;
        public static IModel channel;

        public BlaiseNISRACaseProcessor()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            Run();
        }

        protected override void OnStart(string[] args)
        {
            // Setup Quartz job.
			log.Info("Blaise NISRA Case Processor service started.");
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
            _scheduler.Start();
            AddJob();
        }

        public static void AddJob()
        {
            string quartzCron = ConfigurationManager.AppSettings["QuartzCron"];
            log.Info("Quartz Cron - " + quartzCron);
            IDoJob job = new BlaiseNISRACaseProcessorJob();
            var jobDetail = new JobDetailImpl("job", "group", job.GetType());
            var triggerDetail = new CronTriggerImpl("trigger", "group", quartzCron);
            _scheduler.ScheduleJob(jobDetail, triggerDetail);
        }

        public class BlaiseNISRACaseProcessorJob : IDoJob
        {
            public void Execute(IJobExecutionContext context)
            {
                log.Info("Quartz job triggered.");
                Run();
            }
        }

        protected override void OnStop()
        {
            log.Info("Blaise NISRA Case Processor service stopped.");
        }

        public static void Run()
        {
            // Get environment variables.
            string bucketName = ConfigurationManager.AppSettings["BucketName"];
            log.Info("bucketName - " + bucketName);
            string localProcessFolder = ConfigurationManager.AppSettings["LocalProcessFolder"];
            log.Info("localProcessFolder - " + localProcessFolder);
            string serverName = ConfigurationManager.AppSettings["BlaiseServerHostName"];
            log.Debug("BlaiseServerHostName - " + serverName);
            string userName = ConfigurationManager.AppSettings["BlaiseServerUserName"];
            log.Debug("BlaiseServerUserName - " + userName);
            string password = ConfigurationManager.AppSettings["BlaiseServerPassword"];
            log.Debug("BlaiseServerPassword - " + password);
            string binding = ConfigurationManager.AppSettings["BlaiseServerBinding"];
            log.Debug("BlaiseServerBinding - " + binding);

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            var credFilePath = @"C:\dev\cred.json";
            var googleCredStream = GoogleCredential.FromStream(File.OpenRead(credFilePath));
            var bucket = StorageClient.Create(googleCredStream);

#else
            // When running in Release, the service will be running as compute account which will have access to all buckets.
            var bucket = StorageClient.Create();
#endif

            // Copy objects/files inside bucket to local processing folder.
            log.Info("Checking bucket contents.");
            foreach (var bucketObj in bucket.ListObjects(bucketName, ""))
            {
                // Ignore processed and audit objects/files.
                if (!bucketObj.Name.ToLower().Contains("processed") && !bucketObj.Name.ToLower().Contains("audit"))
                {
                    log.Info("Object/file found - " + bucketObj.Name);
                    string localProcessFilePath = localProcessFolder + "/" + bucketObj.Name;
                    log.Info("Creating local folder structure - " + (Path.GetDirectoryName(localProcessFilePath)));
                    DirectoryInfo dirInfo = Directory.CreateDirectory(Path.GetDirectoryName(localProcessFilePath));
                    var outputFile = File.OpenWrite(localProcessFilePath);
                    log.Info("Copying object/file locally - " + outputFile.Name);
                    bucket.DownloadObject(bucketName, bucketObj.Name, outputFile);
                    outputFile.Close();
                }
            }

            // Close connection to bucket.
            bucket.Dispose();

            // Search for all files in process folder and subfolders and move to root.
            log.Info("Moving files to local process folder root.");
            void RecurDirSearch(string topDir)
            {
                try
                {
                    foreach (string dir in Directory.GetDirectories(topDir))
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            var fileName = Path.GetFileName(file);
                            var destFile = Path.Combine(localProcessFolder, fileName);
                            File.Delete(destFile);
                            File.Move(file, destFile);
                            log.Info("File moved - " + file + " > " + destFile);
                        }
                        RecurDirSearch(dir);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Error searching through files in local process folder.");
                    log.Error(e.Message);
                    log.Error(e.StackTrace);
                }
            }
            RecurDirSearch(localProcessFolder);

            var bdixFiles = new List<string>();

            // Look for BDIX files in the local NISRA processing folder.
            if (Directory.Exists(localProcessFolder))
            {
                log.Info("Checking for BDIX files.");
                var allFiles = Directory.GetFiles(localProcessFolder, "*.bdix", SearchOption.TopDirectoryOnly);
                foreach(var file in allFiles)
                {
                    bdixFiles.Add(file);
                }
            }
            else
            {
                log.Warn("Process folder doesn't exist - " + localProcessFolder);
            }

            // If a BDIX file is found, process it.
            if (bdixFiles != null && bdixFiles.Count != 0)
            {
                // Process all the BDIX files found.
                foreach (var bdixFile in bdixFiles)
                {
                    log.Info("Processing NISRA file - " + bdixFile);
                    // Check BDBX is not locked before processing.
                    var bdbxFile = bdixFile.Substring(0, bdixFile.Length - 4) + "bdbx";
                    if (FileMethods.CheckFileLock(bdbxFile) == false)
                    {                        
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
                                if (survey.Name.ToUpper() == Path.GetFileNameWithoutExtension(bdixFile).ToUpper())
                                {
                                    log.Info("Survey found on server (" + serverPark.Name + "/" + survey.Name + ") that matches NISRA file.");
                                    ProcessData(serverPark, survey);
                                }
                                else
                                {
                                    log.Warn("No survey found on Blaise server that matches NISRA file.");
                                }
                            }
                        }
                    }
                    else
                    {
                        log.Info("Unable to process bdbx due to lock on file - " + bdbxFile);
                    }                    
                }
            }
            else
            {
                log.Info("No BDIX files found.");
            }
        }

        /// <summary>
        /// Process the survey data.
        /// </summary>
        public static void ProcessData(IServerPark serverPark, ISurvey instrument)
        {
            try
            {
                log.Info("Processing data for survey " + instrument.Name + " on server park " + serverPark.Name + ".");
                // Get process and backup folder locations from app config.
                string bucketName = ConfigurationManager.AppSettings["BucketName"];
                log.Info("bucketName - " + bucketName);
                string localProcessFolder = ConfigurationManager.AppSettings["LocalProcessFolder"];
                log.Info("localProcessFolder - " + localProcessFolder);
                
                // Get path for NISRA bdix.
                string nisraBDI = BlaiseMethods.GetBDIFile(localProcessFolder, instrument.Name);
                // Get data links for the NISRA file and Blaise server.
                var nisraFileDataLink = BlaiseMethods.GetDataLinkFromBDI(nisraBDI);
                var blaiseServerDataLink = BlaiseMethods.GetRemoteDataLink(serverPark, instrument);
                // If we have data links for the NISRA file and the Blaise server.
                if (nisraFileDataLink != null || blaiseServerDataLink != null)
                {
                    // Attempt to import the cases.
                    if (ImportDataRecords(nisraFileDataLink, blaiseServerDataLink, instrument, serverPark))
                    {
                        // Move the NISRA files to the processed location if they've been sucessfully processed.
                        log.Info("Moving processed files to processed location.");
                        // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
                        // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
                        var credFilePath = @"C:\dev\cred.json";
                        var googleCredStream = GoogleCredential.FromStream(File.OpenRead(credFilePath));
                        var bucket = StorageClient.Create(googleCredStream);

#else
                        // When running in Release, the service will be running as compute account which will have access to all buckets.
                        var bucket = StorageClient.Create();
#endif

                        foreach (var storageObject in bucket.ListObjects(bucketName, ""))
                        {
                            if (storageObject.Name.ToLower().Contains(instrument.Name.ToLower() + ".b"))
                            {
                                // Remove up to last slash.
                                int endIndex = storageObject.Name.LastIndexOf("/");
                                endIndex = endIndex != -1 ? endIndex : 0;
                                var filePath = storageObject.Name.Substring(0, endIndex);
                                var dest = filePath + "/processed/" + Path.GetFileName(storageObject.Name);
                                // Move file/object.
                                bucket.CopyObject(bucketName, storageObject.Name, bucketName, dest);
                                bucket.DeleteObject(bucketName, storageObject.Name);
                                log.Info("File/object moved - " + storageObject.Name + " > " + dest);
                            }
                        }

                        // Close connection to bucket.
                        bucket.Dispose();

                        // Delete local process folder.
                        void DeleteDirectory(string topDir)
                        {
                            string[] files = Directory.GetFiles(topDir);
                            string[] dirs = Directory.GetDirectories(topDir);
                            foreach (string file in files)
                            {
                                File.SetAttributes(file, FileAttributes.Normal);
                                File.Delete(file);
                            }
                            foreach (string dir in dirs)
                            {
                                DeleteDirectory(dir);
                            }
                            Directory.Delete(topDir, false);
                            log.Info("Folder deleted - " + topDir);
                        }
                        DeleteDirectory(localProcessFolder);
                    }
                }
                else
                {
                    log.Warn("Could not get data links for NISRA file and/or Blaise server.");
                }
            }
            catch (Exception e)
            {
                log.Error($"Error Processing survey, ServerPark: {serverPark.Name}, InstrumentName: {instrument.Name}");
                log.Error(e.Message);
                log.Error(e.StackTrace);
            }
        }

        /// <summary>
        /// Uses two datalink objects to import the data from one source to another.
        /// </summary>
        /// <param name="sourceDL">A datalink object referencing the source data location.</param>
        /// <param name="targetDL">A datalink object referencing the target data location.</param>
        public static bool ImportDataRecords(IDataLink nisraDatalink, IDataLink4 serverDataLink, ISurvey instrument, IServerPark serverPark)
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
                    // Read the current record.
                    var nisraRecord = nisraDataSet.ActiveRecord;
                    // Get the key field from the model.
                    IDatamodel sourceModel = nisraRecord.Datamodel;
                    var key = DataRecordManager.GetKey(sourceModel, "PRIMARY");
                    // Get the value of the key field.
                    string serialNumber = nisraRecord.Keys[0].KeyValue;
                    key.Fields[0].DataValue.Assign(serialNumber);
					log.Info("Processing NISRA record with serial number: " + serialNumber + ".");
                    // Check if a record with the key field value exists on the Blaise server.
                    if (serverDataLink.KeyExists(key))
                    {
                        log.Info("Matching record found on Blaise server.");
						// Get the record on the server.
                        var serverRecord = serverDataLink.ReadRecord(key);

                        // Check if the WebFormStatus field exists in the NISRA and server record
                        if (DataRecordMethods.CheckForField(nisraRecord, "WebFormStatus") && DataRecordMethods.CheckForField(serverRecord, "WebFormStatus"))
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
                            // Otherwise write the completed NISRA record to the server.
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
                                    var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA record imported");

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
                                    var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA record imported");

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
                        // If no record is found, write the record straight to the Blaise server (IPS behaviour).
                        log.Info("NISRA record with serial number: " + serialNumber + " not on server park. Writing record to server park.");
                        serverDataLink.Write(nisraRecord);
                    }
                    // Move to the next record:
                    nisraDataSet.MoveNext();
                }
                if (connection != null && connection.IsOpen == true)
                {
                    connection.Close();
                }                
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

        public static void ProcessRecordHoutValues(StatNeth.Blaise.API.DataRecord.IDataRecord nisraRecord, StatNeth.Blaise.API.DataRecord.IDataRecord serverRecord, IDataLink4 serverDataLink, 
                                            ISurvey instrument, IServerPark serverPark, string serialNumber)
        {
            // Check for an HOUT field in the NISRA data.
            if (DataRecordMethods.CheckForField(nisraRecord, "QHAdmin.HOut"))
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
                        var json = MakeJsonStatus(nisraRecord, instrument.Name, serverPark.Name, "NISRA record imported");

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

        public static void WriteNisraRecordToServer(StatNeth.Blaise.API.DataRecord.IDataRecord nisraRecord, StatNeth.Blaise.API.DataRecord.IDataRecord serverRecord, IDataLink4 serverDataLink)
        {
            // Get the Case_ID field objects for NISRA and the server.
            var serverCaseID = serverRecord.GetField("QID.Case_ID");
            var nisraCaseID = nisraRecord.GetField("QID.Case_ID");

            // Before we update the server record/case, take the Case_ID from the server data and put it in the NISRA data.
            // This is so that if the NISRA data doesn't have the Case_ID, it's not lost.
            nisraCaseID.DataValue.Assign(serverCaseID.DataValue.ValueAsText);

            // Modify the Online flag to indicate the new record is from the NISRA data set
            nisraRecord = DataRecordMethods.AssignValueIfFieldExists(nisraRecord, "QHAdmin.Online", "1");

            // Update the server data with the NISRA data.
            serverDataLink.Write(nisraRecord);
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
        public static bool MoveFiles(string sourcePath, string targetPath, MoveType moveType, string instrumentName)
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
                        string fileName = Path.GetFileName(file);

                        if (fileName.ToUpper().Contains(instrumentName.ToUpper()))
                        {

                            string destFile = Path.Combine(targetPath, fileName);

                            Directory.CreateDirectory(targetPath);
                            switch (moveType)
                            {
                                case MoveType.Move:
                                    File.Delete(destFile);
                                    File.Move(file, destFile);
                                    log.Info(String.Format("Successfully moved file - {0} > {1} ({2})", sourcePath, targetPath, fileName));
                                    break;
                                case MoveType.Copy:
                                    File.Copy(file, destFile, true);
                                    log.Info(String.Format("Successfully copied file - {0} > {1} ({2})", sourcePath, targetPath, fileName));
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
        public static bool SetupRabbit()
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
        public static Dictionary<string, string> MakeJsonStatus(StatNeth.Blaise.API.DataRecord.IDataRecord recordData, string instrumentName, string serverPark, string status)
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
        private static void SendStatus(Dictionary<string, string> jsonData)
        {
            string message = new JavaScriptSerializer().Serialize(jsonData);
            var body = Encoding.UTF8.GetBytes(message);
            string caseStatusQueueName = ConfigurationManager.AppSettings["CaseStatusQueueName"];
            channel.BasicPublish(exchange: "", routingKey: caseStatusQueueName, body: body);
            log.Info("Message sent to RabbitMQ " + caseStatusQueueName + " queue - " + message);
        }
        internal interface IDoJob : IJob
        {
        }
    }
}
