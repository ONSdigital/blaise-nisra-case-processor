using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;

namespace BlaiseNisraCaseProcessor.Services
{
    public class QueueService : IQueueService
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFluentQueueApi _queueProvider;

        public QueueService(
            IConfigurationProvider configurationProvider,
            IFluentQueueApi queueProvider)
        {
            _configurationProvider = configurationProvider;
            _queueProvider = queueProvider;
        }

        public void PublishMessage(string message)
        {
            _queueProvider
                .WithProject(_configurationProvider.ProjectId)
                .WithTopic(_configurationProvider.TopicId)
                .Publish(message);
        }
    }
}
