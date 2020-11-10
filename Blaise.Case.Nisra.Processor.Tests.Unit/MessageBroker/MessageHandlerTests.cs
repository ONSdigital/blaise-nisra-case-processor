using System;
using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker;
using Blaise.Case.Nisra.Processor.MessageBroker.Enums;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Model;
using log4net;
using Moq;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.MessageBroker
{
    public class MessageHandlerTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IStorageService> _storageServiceMock;
        private Mock<IProcessNisraFilesService> _processFilesServiceMock;
        private Mock<IMessageModelMapper> _mapperMock;

        private readonly string _message;
        private readonly MessageModel _messageModel;
        private readonly List<string> _availableFilesFromBucket;
        private readonly List<string> _downloadedFilesFromBucket;

       MessageHandler _sut;


        public MessageHandlerTests()
        {
            _message = "Message";
            _messageModel = new MessageModel { Action = ActionType.Process};
            _availableFilesFromBucket = new List<string> { "File1.bdbx", "File2.bdbx" };
            _downloadedFilesFromBucket = new List<string> { "File1.bdbx", "File2.bdbx" };
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();

            _storageServiceMock = new Mock<IStorageService>();

            _processFilesServiceMock = new Mock<IProcessNisraFilesService>();

            _mapperMock = new Mock<IMessageModelMapper>();
            _mapperMock.Setup(m => m.MapToNisraCaseActionModel(_message)).Returns(_messageModel);

            _sut = new MessageHandler(
                _loggingMock.Object,
                _storageServiceMock.Object,
                _processFilesServiceMock.Object,
                _mapperMock.Object);
        }

        [Test]
        public void Given_Process_Action_Is_Not_Set_When_I_Call_HandleMessage_Then_True_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);

            _messageModel.Action = ActionType.NotSupported;

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_Process_Action_Is_Not_Set_When_I_Call_HandleMessage_Then_Nothing_Is_Processed()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);

            _messageModel.Action = ActionType.NotSupported;

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.VerifyNoOtherCalls();
            _processFilesServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Files_Are_Available_When_I_Call_HandleMessage_Then_True_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_Files_Are_Available_When_I_Call_HandleMessage_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetListOfAvailableFilesInBucket(), Times.Once);
            _storageServiceMock.Verify(v => v.DownloadFilesFromBucket(_availableFilesFromBucket), Times.Once);
            _processFilesServiceMock.Verify(v => v.ProcessFiles(_downloadedFilesFromBucket), Times.Once);
            _storageServiceMock.Verify(v => v.MoveProcessedFilesToProcessedFolder(_availableFilesFromBucket));

            _storageServiceMock.VerifyNoOtherCalls();
            _processFilesServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_No_Files_Are_Available_in_Bucket_When_I_Call_HandleMessage_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetListOfAvailableFilesInBucket(), Times.Once);
            _processFilesServiceMock.Verify(v => v.ProcessFiles(_availableFilesFromBucket), Times.Never);
        }

        [Test]
        public void Given_No_Files_Are_Available_in_Bucket_When_I_Call_HandleMessage_Then_True_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(new List<string>());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_No_Files_Are_Available_When_I_Call_HandleMessage_Then_Nothing_Is_Processed()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetListOfAvailableFilesInBucket(), Times.Once);

            _storageServiceMock.VerifyNoOtherCalls();
            _processFilesServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_No_Files_Are_Downloaded_From_The_Bucket_When_I_Call_HandleMessage_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetListOfAvailableFilesInBucket(), Times.Once);
            _processFilesServiceMock.Verify(v => v.ProcessFiles(_availableFilesFromBucket), Times.Never);
        }

        [Test]
        public void Given_No_Files_Are_Downloaded_From_The_Bucket_When_I_Call_HandleMessage_Then_True_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(new List<string>());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_No_Files_Are_Downloaded_From_The_Bucket_When_I_Call_HandleMessage_Then_Nothing_Is_Processed()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _storageServiceMock.Verify(v => v.GetListOfAvailableFilesInBucket(), Times.Once);
            _storageServiceMock.Verify(v => v.DownloadFilesFromBucket(_availableFilesFromBucket), Times.Once);

            _storageServiceMock.VerifyNoOtherCalls();
            _processFilesServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_An_Error_Occurs_When_Getting_Files_From_The_Bucket_When_I_Call_HandleMessage_Then_False_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Throws(new Exception());

            //act 
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Error_Occurs_When_Getting_Files_From_The_Bucket_When_I_Call_HandleMessage_Then_Exception_Is_Handled_Correctly()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.HandleMessage(_message));
        }

        [Test]
        public void Given_An_Error_Occurs_When_Getting_Files_From_The_Bucket_When_I_Call_HandleMessage_Then_Nothing_Is_Processed()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Throws(new Exception());

            //act && assert
            _sut.HandleMessage(_message);

            _processFilesServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_An_Error_Occurs_When_Processing_Files_When_I_Call_HandleMessage_Then_Exception_Is_Handled_Correctly()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);
            _processFilesServiceMock.Setup(c => c.ProcessFiles(_downloadedFilesFromBucket)).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.HandleMessage(_message));
        }

        [Test]
        public void Given_An_Error_Occurs_When_Processing_Files_When_I_Call_HandleMessage_Then_False_Is_Returned()
        {
            //arrange
            _storageServiceMock.Setup(c => c.GetListOfAvailableFilesInBucket()).Returns(_availableFilesFromBucket);
            _storageServiceMock.Setup(c => c.DownloadFilesFromBucket(_availableFilesFromBucket)).Returns(_downloadedFilesFromBucket);
            _processFilesServiceMock.Setup(c => c.ProcessFiles(_downloadedFilesFromBucket)).Throws(new Exception());

            //act 
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }
    }
}
