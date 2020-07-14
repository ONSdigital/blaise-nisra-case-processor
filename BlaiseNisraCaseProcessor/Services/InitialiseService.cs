using System;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class InitialiseService : IInitialiseService
    {
        private readonly ILog _logger;
        private readonly IQueueService _queueService;
        private readonly IMessageHandler _messageHandler;
        private readonly IConfigurationProvider _configurationProvider;

        public InitialiseService(
            ILog logger,
            IQueueService queueService,
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
            }

            _logger.Info($"Nisra Case processing service started on '{_configurationProvider.VmName}'");
        }

        public void Stop()
        {
            _logger.Info($"Stopping Nisra Case processing service on '{_configurationProvider.VmName}'");

            _queueService.CancelAllSubscriptions();

            _logger.Info($"Nisra Case processing service stopped on '{_configurationProvider.VmName}'");
        }
    }
}