using Blaise.Case.Nisra.Processor.MessageBroker.Enums;
using Blaise.Case.Nisra.Processor.MessageBroker.Mappers;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.MessageBroker
{
    public class MessageModelMapperTests
    {
        private readonly MessageModelMapper _sut;

        public MessageModelMapperTests()
        {
            _sut = new MessageModelMapper();
        }

        [Test]
        public void Given_A_Message_With_A_Process_Action_When_I_Call_MapToNisraCaseActionModel_Then_I_Get_An_Expected_Model_Back()
        {
            //arrange
            const string message =
                @"{ ""ACTION"":""process""}";

            //act
            var result = _sut.MapToNisraCaseActionModel(message);

            //assert`
            Assert.NotNull(result);
            Assert.IsInstanceOf<MessageModel>(result);
            Assert.AreEqual(ActionType.Process, result.Action);
        }

        [Test]
        public void Given_An_Invalid_Message_With_A_Process_Action_When_I_Call_MapToNisraCaseActionModel_Then_I_Get_A_Default_Model_Back()
        {
            //arrange
            const string message =
                @"{ ""ACTION"":""none""}";

            //act
            var result = _sut.MapToNisraCaseActionModel(message);

            //assert`
            Assert.NotNull(result);
            Assert.IsInstanceOf<MessageModel>(result);
            Assert.AreEqual(ActionType.NotSupported, result.Action);
        }
    }
}
