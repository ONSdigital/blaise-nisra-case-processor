using System.Configuration;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Nisra.Case.Processor.Tests.Behaviour.Helpers
{
    public class PubSubHelper
    {
        private readonly IFluentQueueApi _queueApi;

        private readonly string _projectId;

        private readonly string _topicId;

        public PubSubHelper()
        {
            _queueApi = new FluentQueueApi();

            _projectId = ConfigurationManager.AppSettings["ProjectId"];
            _topicId = ConfigurationManager.AppSettings["TopicId"];
        }

        public void PublishMessage(string message)
        {
            
            _queueApi
                .WithProject(_projectId)
                .WithTopic(_topicId)
                .Publish(message);

        }
    }
}
