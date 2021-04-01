using Blaise.Case.Nisra.Processor.ConsoleService.Ioc;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.ConsoleService
{
    class Program
    {
        static void Main(string[] args)
        {
            var unityProvider = new UnityProvider();
            var messageBroker = unityProvider.Resolve<IMessageBrokerService>();
            var messageHandler = unityProvider.Resolve<IMessageTriggerHandler>();
            messageBroker.Subscribe(messageHandler);
        }
    }
}
