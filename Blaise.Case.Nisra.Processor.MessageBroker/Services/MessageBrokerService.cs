using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Services
{
    public class MessageBrokerService : IMessageBrokerService
    {
        private readonly ILoggingService _loggingService;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFluentQueueApi _queueApi;

        public MessageBrokerService(
            ILoggingService loggingService,
            IConfigurationProvider configurationProvider,
            IFluentQueueApi queueApi)
        {
            _loggingService = loggingService;
            _configurationProvider = configurationProvider;
            _queueApi = queueApi;
        }

        public void Subscribe(IMessageTriggerHandler messageHandler)
        {
            _queueApi
                .WithProject(_configurationProvider.ProjectId)
                .WithSubscription(_configurationProvider.SubscriptionId)
                .StartConsuming(messageHandler);

            _loggingService.LogInfo($"Subscription setup to '{_configurationProvider.SubscriptionId}' " +
                         $"for project '{_configurationProvider.ProjectId}'");
        }

        public void CancelAllSubscriptions()
        {
            _queueApi.StopConsuming();
            
            _loggingService.LogInfo($"Stopped consuming Subscription to '{_configurationProvider.SubscriptionId}'" +
                                    $" for project '{_configurationProvider.ProjectId}'");
        }
    }
}
