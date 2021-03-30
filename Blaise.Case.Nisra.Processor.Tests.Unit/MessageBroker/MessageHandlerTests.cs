using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Handler;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using Moq;
using NUnit.Framework;
using System;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.MessageBroker
{
    public class MessageHandlerTests
    {
        private Mock<ILoggingService> _loggingMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<IImportNisraDataFileService> _nisraDataServiceMock;
        private Mock<IMessageModelMapper> _mapperMock;

        private readonly string _message;
        private readonly MessageModel _messageModel;

        private MessageHandler _sut;


        public MessageHandlerTests()
        {
            _message = "Message";
            _messageModel = new MessageModel
            {
                BucketPath = "Instruments\\OPN2101A",
                InstrumentName = "OPN2101A",
                ServerParkName = "Gusty"
            };
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILoggingService>();
            _storageServiceMock = new Mock<IStorageService>();
            _nisraDataServiceMock = new Mock<IImportNisraDataFileService>();

            _mapperMock = new Mock<IMessageModelMapper>();
            _mapperMock.Setup(m => m.MapToMessageModel(_message)).Returns(_messageModel);

            _sut = new MessageHandler(
                _loggingMock.Object,
                _storageServiceMock.Object,
                _nisraDataServiceMock.Object,
                _mapperMock.Object);
        }

        [Test]
        public void Given_A_Valid_Message_When_I_Call_HandleMessage_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            const string databaseFile = @"d:\temp\OPN2101A.bdix";
            _storageServiceMock.Setup(s => s.GetInstrumentFileFromBucket(_messageModel.InstrumentName,
                _messageModel.BucketPath)).Returns(databaseFile);

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetInstrumentFileFromBucket(_messageModel.InstrumentName, 
                _messageModel.BucketPath), Times.Once);

            _nisraDataServiceMock.Verify(v => v.ImportNisraDatabaseFile(_messageModel.ServerParkName,
                _messageModel.InstrumentName, databaseFile), Times.Once);

            _storageServiceMock.Verify(s => s.DeleteDownloadedFiles());
        }

        [Test]
        public void Given_An_Exception_Occurs_When_I_Call_HandleMessage_Then_The_Exception_Is_Handled_And_Logged()
        {
            //arrange
            _storageServiceMock.Setup(s => s.GetInstrumentFileFromBucket(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new Exception());

            //act
            Assert.DoesNotThrow(()=> _sut.HandleMessage(_message));

            //assert
            _loggingMock.Verify(v => v.LogError(It.IsAny<string>(), It.IsAny<Exception>()));
        }
    }
}
