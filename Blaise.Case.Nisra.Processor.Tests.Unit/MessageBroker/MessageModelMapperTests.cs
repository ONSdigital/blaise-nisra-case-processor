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
        public void Given_A_Valid_Message_When_I_Call_MapToMessageModel_Then_I_Get_An_Expected_Model_Back()
        {
            //arrange
            const string message = @"{""server_park_name"":""gusty"", ""instrument_name"":""OPN2101A"", ""bucket_path"":""instruments\\OPN2101A""}";

            //act
            var result = _sut.MapToMessageModel(message);

            //assert`
            Assert.NotNull(result);
            Assert.IsInstanceOf<MessageModel>(result);
            Assert.AreEqual("gusty", result.ServerParkName);
            Assert.AreEqual("OPN2101A", result.InstrumentName);
            Assert.AreEqual("instruments\\OPN2101A", result.BucketPath);
        }
    }
}
