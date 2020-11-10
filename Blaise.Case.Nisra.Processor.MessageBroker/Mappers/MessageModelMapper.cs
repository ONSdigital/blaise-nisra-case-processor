using Blaise.Case.Nisra.Processor.MessageBroker.Enums;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using Newtonsoft.Json;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Mappers
{
    public class MessageModelMapper : IMessageModelMapper
    {
        public MessageModel MapToNisraCaseActionModel(string message)
        {
            try
            {
                return JsonConvert.DeserializeObject<MessageModel>(message);
            }
            catch 
            {
                // This is horrible I know but we currently don't really care about the message as it is only a trigger
                // and we need to ensure a message incorrectly put on this topic does not trigger it
            }

            return new MessageModel { Action = ActionType.NotSupported };
        }
    }
}
