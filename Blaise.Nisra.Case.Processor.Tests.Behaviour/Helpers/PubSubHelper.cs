using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class PubSubHelper
    {
        private readonly IFluentQueueApi _queueApi;
        private readonly ConfigurationHelper _configurationHelper;

        public PubSubHelper()
        {
            _queueApi = new FluentQueueApi();
            _configurationHelper = new ConfigurationHelper();
        }

        public void PublishMessage(string message)
        {
            
            _queueApi
                .WithProject(_configurationHelper.ProjectId)
                .WithTopic(_configurationHelper.TopicId)
                .Publish(message);

        }
    }
}
