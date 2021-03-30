
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Interfaces
{
    public interface IMessageBrokerService
    {
        void Subscribe(IMessageTriggerHandler messageHandler);

        void CancelAllSubscriptions();
    }
}