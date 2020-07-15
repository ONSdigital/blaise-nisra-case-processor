using Blaise.Nuget.Api.Contracts.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;
using CaseStatusType = BlaiseNisraCaseProcessor.Enums.CaseStatusType;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class UpdateCaseServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IUpdateCaseByHoutService> _updateCasedByHoutServiceMock;
        private Mock<IPublishCaseStatusService> _publishCaseStatusServiceMock;

        private Mock<IDataRecord> _newDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;

        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private UpdateRecordService _sut;

        public UpdateCaseServiceTests()
        {
            _serialNumber = "SN123";
            _serverParkName = "Park1";
            _surveyName = "OPN123";
        }

        [SetUp]
        public void SetUpTests()
        {
            _newDataRecordMock = new Mock<IDataRecord>();
            _existingDataRecordMock = new Mock<IDataRecord>();

            _loggingMock = new Mock<ILog>();

            _blaiseApiServiceMock = new Mock<IBlaiseApiService>();

            _updateCasedByHoutServiceMock = new Mock<IUpdateCaseByHoutService>();

            _publishCaseStatusServiceMock = new Mock<IPublishCaseStatusService>();

            _sut = new UpdateRecordService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _updateCasedByHoutServiceMock.Object,
                _publishCaseStatusServiceMock.Object);
        }

        [Test]
        public void Given_The_Nisra_WebFormStatus_Is_Set_As_NotProcessed_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.NotProcessed);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Complete_And_The_Nisra_WebFormStatus_Is_Complete_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Updated_By_HOut()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.Complete);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.Complete);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _updateCasedByHoutServiceMock.Verify(v => v.UpdateCaseByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Complete, WebFormStatusType.NotSpecified)]
        [TestCase(WebFormStatusType.Complete, WebFormStatusType.Partial)]
        public void Given_Existing_WebFormStatus_Is_Complete_And_The_Nisra_WebFormStatus_Is_Not_Complete_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Not_Updated(
            WebFormStatusType existingWebFormStatusType, WebFormStatusType newWebFormStatusType)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(existingWebFormStatusType);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(newWebFormStatusType);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Partial_And_The_Nisra_WebFormStatus_Is_Partial_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Updated_By_HOut()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.Partial);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.Partial);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _updateCasedByHoutServiceMock.Verify(v => v.UpdateCaseByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Partial, WebFormStatusType.NotSpecified)]
        [TestCase(WebFormStatusType.Partial, WebFormStatusType.Complete)]
        public void Given_Existing_WebFormStatus_Is_Partial_And_The_Nisra_WebFormStatus_Is_Not_Partial_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Updated(
            WebFormStatusType existingWebFormStatusType, WebFormStatusType newWebFormStatusType)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(existingWebFormStatusType);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(newWebFormStatusType);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Partial, WebFormStatusType.NotSpecified)]
        [TestCase(WebFormStatusType.Partial, WebFormStatusType.Complete)]
        public void Given_Existing_WebFormStatus_Is_Partial_And_The_Nisra_WebFormStatus_Is_Not_Partial_When_I_Call_UpdateCase_Then_A_Message_Is_Published(
            WebFormStatusType existingWebFormStatusType, WebFormStatusType newWebFormStatusType)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(existingWebFormStatusType);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(newWebFormStatusType);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_newDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Not_Partial_Or_Complete_And_The_Nisra_WebFormStatus_Is_NoTSpecified_When_I_Call_UpdateCase_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.NotProcessed);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.NotSpecified);

            //act
            _sut.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateCasedByHoutServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }
    }
}
