using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using Newtonsoft.Json;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Mappers
{
    public class MessageModelMapper : IMessageModelMapper
    {
        public MessageModel MapToMessageModel(string message)
        {
            return JsonConvert.DeserializeObject<MessageModel>(message);
        }
    }
}
