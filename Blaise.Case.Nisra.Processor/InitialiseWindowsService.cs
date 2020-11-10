using System;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker;
using Blaise.Case.Nisra.Processor.WindowsService.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;

namespace Blaise.Case.Nisra.Processor.WindowsService
{
    public class InitialiseWindowsService : IInitialiseWindowsService
    {
        private readonly ILog _logger;
        private readonly IMessageBrokerService _queueService;
        private readonly IMessageHandler _messageHandler;
        private readonly IConfigurationProvider _configurationProvider;

        public InitialiseWindowsService(
            ILog logger,
            IMessageBrokerService queueService,
            IMessageHandler messageHandler,
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _queueService = queueService;
            _messageHandler = messageHandler;
            _configurationProvider = configurationProvider;
        }

        public void Start()
        {
            _logger.Info($"Starting Nisra Case processing service on '{_configurationProvider.VmName}'");

            try
            {
                _queueService.Subscribe(_messageHandler);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn($"There was an error starting the Data Delivery service");
                throw;
            }

            _logger.Info($"Nisra Case processing service started on '{_configurationProvider.VmName}'");
        }

        public void Stop()
        {
            _logger.Info($"Stopping Nisra Case processing service on '{_configurationProvider.VmName}'");

            try
            {
                _queueService.CancelAllSubscriptions();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn($"There was an error stopping the Nisra Case processing service");
                throw;
            }

            _logger.Info($"Nisra Case processing service stopped on '{_configurationProvider.VmName}'");
        }
    }
}