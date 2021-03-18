using System;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.WindowsService.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;


namespace Blaise.Case.Nisra.Processor.WindowsService
{
    public class InitialiseWindowsService : IInitialiseWindowsService
    {
        private readonly ILoggingService _loggingService;
        private readonly IMessageBrokerService _queueService;
        private readonly IMessageHandler _messageHandler;

        public InitialiseWindowsService(
            ILoggingService loggingService,
            IMessageBrokerService queueService,
            IMessageHandler messageHandler)
        {
            _loggingService = loggingService;
            _queueService = queueService;
            _messageHandler = messageHandler;
        }

        public void Start()
        {
            _loggingService.LogInfo("Starting Nisra Case processing service");

            try
            {
                _queueService.Subscribe(_messageHandler);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("There was an error starting the Nisra Case processing service", ex);
                throw;
            }

            _loggingService.LogInfo("Nisra Case processing service started");
        }

        public void Stop()
        {
            _loggingService.LogInfo("Stopping Nisra Case processing service");

            try
            {
                _queueService.CancelAllSubscriptions();
            }
            catch (Exception ex)
            {
                _loggingService.LogError("There was an error stopping the Nisra Case processing service", ex);
                throw;
            }

            _loggingService.LogInfo("Nisra Case processing service stopped");
        }
    }
}