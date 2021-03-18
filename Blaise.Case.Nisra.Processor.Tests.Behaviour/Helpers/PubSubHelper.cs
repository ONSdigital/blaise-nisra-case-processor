using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.Tests.Behaviour.Helpers
{
    public class PubSubHelper
    {
        private static PubSubHelper _currentInstance;
        private readonly IFluentQueueApi _queueApi;

        public PubSubHelper()
        {
            _queueApi = new FluentQueueApi();
        }
        
        public static PubSubHelper GetInstance()
        {
            return _currentInstance ?? (_currentInstance = new PubSubHelper());
        }
        
        public void PublishMessage(string message)
        {
            
            _queueApi
                .WithProject(BlaiseConfigurationHelper.ProjectId)
                .WithTopic(BlaiseConfigurationHelper.TopicId)
                .Publish(message);

        }
    }
}
