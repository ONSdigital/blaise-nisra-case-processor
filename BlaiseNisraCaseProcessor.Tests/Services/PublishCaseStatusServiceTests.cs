using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class PublishCaseStatusServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<ICaseMapper> _mapperMock;
        private Mock<IQueueService> _queueServiceMock;
        private readonly Mock<IDataRecord> _dataRecordMock;

        private readonly string _surveyName;
        private readonly string _serverPark;

        private PublishCaseStatusService _sut;

        public PublishCaseStatusServiceTests()
        {
            _surveyName = "InstrumentName";
            _serverPark = "ServerPark";

            _dataRecordMock = new Mock<IDataRecord>();
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();
            _mapperMock = new Mock<ICaseMapper>();

            _queueServiceMock = new Mock<IQueueService>();
            _queueServiceMock.Setup(q => q.PublishMessage(It.IsAny<string>()));


            _sut = new PublishCaseStatusService(
                _loggingMock.Object,
                _queueServiceMock.Object,
                _mapperMock.Object);
        }

        [TestCase(CaseStatusType.NisraCaseImported)]
        public void Given_I_Call_PublishCaseStatus_Then_The_Correct_Services_Are_Called(CaseStatusType caseStatusType)
        {
            //arrange
            var message = "Test Message";
            _mapperMock.Setup(m => m.MapToSerializedJson(
                It.IsAny<IDataRecord>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<CaseStatusType>()))
                .Returns(message);

            _queueServiceMock.Setup(q => q.PublishMessage(It.IsAny<string>()));

            //act
            _sut.PublishCaseStatus(_dataRecordMock.Object, _surveyName, _serverPark, caseStatusType);

            //assert
            _mapperMock.Verify(v => v.MapToSerializedJson(_dataRecordMock.Object, _surveyName, _serverPark, caseStatusType), Times.Once);
            _queueServiceMock.Verify(v => v.PublishMessage(message), Times.Once);
        }
    }
}
