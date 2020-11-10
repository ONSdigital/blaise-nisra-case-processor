
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.MessageBroker
{
    public interface IMessageBrokerService
    {
        void Subscribe(IMessageHandler messageHandler);

        void CancelAllSubscriptions();
    }
}