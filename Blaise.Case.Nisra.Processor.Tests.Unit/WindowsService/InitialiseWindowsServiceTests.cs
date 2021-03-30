using System;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Case.Nisra.Processor.WindowsService;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.WindowsService
{
    public class InitialiseWindowsServiceTests
    {
        private Mock<ILoggingService> _loggingMock;
        private Mock<IMessageBrokerService> _messageBrokerServiceMock;
        private Mock<IMessageTriggerHandler> _messageHandlerMock;

        private InitialiseWindowsService _sut;


        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILoggingService>();
            _messageBrokerServiceMock = new Mock<IMessageBrokerService>();
            _messageHandlerMock = new Mock<IMessageTriggerHandler>();

            _sut = new InitialiseWindowsService(
                _loggingMock.Object,
                _messageBrokerServiceMock.Object,
                _messageHandlerMock.Object);
        }

        [Test]
        public void Given_I_Call_Start_Then_The_Correct_Methods_Are_Called_To_Setup_And_Subscribe_To_The_Appropriate_Queues()
        {
            //act
            _sut.Start();

            //assert
            _messageBrokerServiceMock.Verify(v => v.Subscribe(It.IsAny<IMessageTriggerHandler>()), Times.Once);
        }

        [Test]
        public void Given_I_Call_Start_And_An_Exception_Is_Thrown_During_The_Process_Then_The_Exception_Is_Logged()
        {
            //arrange
            var exceptionThrown = new Exception("Error message");
            _messageBrokerServiceMock.Setup(s => s.Subscribe(It.IsAny<IMessageTriggerHandler>())).Throws(exceptionThrown);
            _loggingMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));

            //act
            Assert.Throws<Exception>(() => _sut.Start());

            //assert
            _loggingMock.Verify(v => v.LogError(It.IsAny<string>(), exceptionThrown), Times.Once);
        }

        [Test]
        public void Given_I_Call_Stop_Then_The_Appropriate_Service_Is_Called()
        {
            //act
            _sut.Stop();

            //assert
            _messageBrokerServiceMock.Verify(v => v.CancelAllSubscriptions(), Times.Once);
        }

        [Test]
        public void Given_I_Call_Stop_And_An_Exception_Is_Thrown_During_The_Process_Then_The_Exception_Is_Logged()
        {
            //arrange
            var exceptionThrown = new Exception("Error message");
            _messageBrokerServiceMock.Setup(s => s.CancelAllSubscriptions()).Throws(exceptionThrown);
            _loggingMock.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));

            //act
            Assert.Throws<Exception>(() => _sut.Stop());

            //assert
            _loggingMock.Verify(v => v.LogError(It.IsAny<string>(), exceptionThrown), Times.Once);
        }
    }
}
