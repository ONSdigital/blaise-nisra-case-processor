
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Nisra.Case.Processor.Interfaces.Services
{
    public interface IQueueService
    {
        void Subscribe(IMessageHandler messageHandler);

        void CancelAllSubscriptions();
    }
}