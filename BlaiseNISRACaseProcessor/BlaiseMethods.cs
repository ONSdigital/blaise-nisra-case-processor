using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.ServerManager;

namespace BlaiseNisraCaseProcessor
{
    public static class BlaiseMethods
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Searches for a .bdi file matching the instrument name in the source directory provided.
        /// </summary>
        /// <param name="sourceDirectory"> The directory where the target BDI file exists. </param>
        /// <param name="instrument"> The name of the instrument (i.e OPN1901A). </param>
        /// <returns> String representation of the file path. </returns>
        public static string GetBDIFile(string sourceDirectory, string instrument = "")
        {
            try
            {
                List<string> ext = new List<string> { ".BDI", ".BDIX" };
                var bdiFiles = Directory.GetFiles(sourceDirectory, String.Format("*{0}.*", instrument), SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).ToUpper()));
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
        /// Gets and returns a datalink connection to the target data file (.bdix).
        /// </summary>
        /// <param name="bdiFile">The name of the target data file (.bdix).</param>
        /// <returns>A IDatalink connection object user to access the stored Blaise data.</returns>
        public static IDataLink GetDataLinkFromBDI(string bdiFile)
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
                IRemoteDataServer dataLinkConn = DataLinkManager.GetRemoteDataServer(serverName, 8033, binding, userName, BlaiseNisraCaseProcessor.GetPassword(password));
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
        /// Gets the name of the data file (.bdix) associated with a specified serverpark and instrument.
        /// </summary>
        /// <param name="serverPark">The serverpark where the instrument exists.</param>
        /// <param name="instrument">The instrument who's data file we're getting.</param>
        /// <returns>The string name of the data file (.bdix)</returns>
        public static string GetDataFileName(string serverPark, string instrument)
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
        /// Establishes a connection to a Blaise Server.
        /// </summary>
        /// <param name="serverName">The location of the Blaise server.</param>
        /// <param name="userName">Username with access to the specified server.</param>
        /// <param name="password">Password for the specified user to access the server.</param>
        /// <returns>A IConnectedServer2 object which is connected to the server provided.</returns>
        public static IConnectedServer2 ConnectToBlaiseServer(string serverName, string userName, string password, string binding)
        {
            int port = 8031;
            try
            {
                IConnectedServer2 connServer =
                    (IConnectedServer2)ServerManager.ConnectToServer(serverName, port, userName, BlaiseNisraCaseProcessor.GetPassword(password), binding);

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
    }
}