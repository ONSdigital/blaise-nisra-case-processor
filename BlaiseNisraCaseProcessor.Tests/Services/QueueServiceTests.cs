using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Services;
using Moq;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class QueueServiceTests
    {
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<IFluentQueueApi> _queueProviderMock;

        private readonly string _projectId;
        private readonly string _topicId;

        private QueueService _sut;

        public QueueServiceTests()
        {
            _projectId = "ProjectId";
            _topicId = "TopicId";
        }

        [SetUp]
        public void SetUpTests()
        {
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(c => c.ProjectId).Returns(_projectId);
            _configurationProviderMock.Setup(c => c.TopicId).Returns(_topicId);


            _queueProviderMock = new Mock<IFluentQueueApi>();

            _sut = new QueueService(
                _configurationProviderMock.Object,
                _queueProviderMock.Object);
        }

        [Test]
        public void Given_I_Call_PublishMessage_Then_The_Message_Is_Published()
        {
            //arrange
            var message = "Test Message";
            _queueProviderMock.Setup(q => q.WithProject(It.IsAny<string>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.WithTopic(It.IsAny<string>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.Publish(It.IsAny<string>(), null));

            //act
            _sut.PublishMessage(message);

            //assert
            _queueProviderMock.Verify(v => v.WithProject(_projectId), Times.Once);
            _queueProviderMock.Verify(v => v.WithTopic(_topicId), Times.Once);
            _queueProviderMock.Verify(v => v.Publish(message, null), Times.Once);
        }
    }
}
