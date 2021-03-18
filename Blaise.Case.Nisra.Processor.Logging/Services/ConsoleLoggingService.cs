using System;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;

namespace Blaise.Case.Nisra.Processor.Logging.Services
{
    public class ConsoleLoggingService : ILoggingService
    {
        public void LogInfo(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarn(string message)
        {
            Console.WriteLine($"Warning - {message}");
        }

        public void LogError(string message, Exception exception)
        {
            Console.WriteLine($"Error - {message}: {exception.Message}, {exception.InnerException}");
        }
    }
}
