using System;
using System.Diagnostics;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;

namespace Blaise.Case.Nisra.Processor.Logging.Services
{
    public class EventLogging : ILoggingService
    {
        public void LogError(string message, Exception exception)
        {
            EventLog.WriteEntry("Rest API", $"RESTAPI: {message}: {exception.Message}, {exception.InnerException}", EventLogEntryType.Error);
        }

        public void LogInfo(string message)
        {
            EventLog.WriteEntry("Rest API", $"RESTAPI: {message}", EventLogEntryType.Information);
        }

        public void LogWarn(string message)
        {
            EventLog.WriteEntry("Rest API", $"RESTAPI: {message}", EventLogEntryType.Warning);
        }
    }
}
