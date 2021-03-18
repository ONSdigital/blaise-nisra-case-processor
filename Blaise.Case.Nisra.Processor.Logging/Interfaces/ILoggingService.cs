using System;

namespace Blaise.Case.Nisra.Processor.Logging.Interfaces
{
    public interface ILoggingService
    {
        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message, Exception exception);
    }
}