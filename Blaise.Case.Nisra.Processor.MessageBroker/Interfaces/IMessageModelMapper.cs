using Blaise.Case.Nisra.Processor.MessageBroker.Model;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Interfaces
{
    public interface IMessageModelMapper
    {
        MessageModel MapToMessageModel(string message);
    }
}
