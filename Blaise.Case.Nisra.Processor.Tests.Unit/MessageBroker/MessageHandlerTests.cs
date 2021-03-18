using System;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Handler;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using log4net;
using Moq;
using NUnit.Framework;

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
        public void Given_A_Valid_Message_When_I_Call_HandleMessage_Then_True_Is_Returned()
        {
            //arrange
            const string databaseFile = @"d:\temp\OPN2101A.bdix";
            _storageServiceMock.Setup(s => s.GetInstrumentFileFromBucket(_messageModel.InstrumentName,
                _messageModel.BucketPath)).Returns(databaseFile);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.True(result);
        }

        [Test]
        public void Given_An_Exception_Occurs_When_I_Call_HandleMessage_Then_False_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(s => s.GetInstrumentFileFromBucket(It.IsAny<string>(),
                It.IsAny<string>())).Throws(new Exception());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.False(result);
        }
    }
}
