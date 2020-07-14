using Blaise.Nuget.Api.Contracts.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class UpdateRecordByWebFormStatusServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IUpdateDataRecordByHoutService> _updateDataRecordByHoutServiceMock;

        private Mock<IDataRecord> _newDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;

        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private UpdateRecordByWebFormStatusService _sut;

        public UpdateRecordByWebFormStatusServiceTests()
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

            _updateDataRecordByHoutServiceMock = new Mock<IUpdateDataRecordByHoutService>();

            _sut = new UpdateRecordByWebFormStatusService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _updateDataRecordByHoutServiceMock.Object);
        }

        [Test]
        public void Given_The_Nisra_WebFormStatus_Is_Set_As_NotProcessed_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.NotProcessed);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Complete_And_The_Nisra_WebFormStatus_Is_Complete_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Updated_By_HOut()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.Complete);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.Complete);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _updateDataRecordByHoutServiceMock.Verify(v => v.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Complete, WebFormStatusType.NotSpecified)]
        [TestCase(WebFormStatusType.Complete, WebFormStatusType.Partial)]
        public void Given_Existing_WebFormStatus_Is_Complete_And_The_Nisra_WebFormStatus_Is_Not_Complete_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Not_Updated(
            WebFormStatusType existingWebFormStatusType, WebFormStatusType newWebFormStatusType)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(existingWebFormStatusType);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(newWebFormStatusType);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Partial_And_The_Nisra_WebFormStatus_Is_Partial_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Updated_By_HOut()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.Partial);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.Partial);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _updateDataRecordByHoutServiceMock.Verify(v => v.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Partial, WebFormStatusType.NotSpecified)]
        [TestCase(WebFormStatusType.Partial, WebFormStatusType.Complete)]
        public void Given_Existing_WebFormStatus_Is_Partial_And_The_Nisra_WebFormStatus_Is_Not_Partial_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Updated(
            WebFormStatusType existingWebFormStatusType, WebFormStatusType newWebFormStatusType)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(existingWebFormStatusType);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(newWebFormStatusType);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateDataRecord(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Existing_WebFormStatus_Is_Not_Partial_Or_Complete_And_The_Nisra_WebFormStatus_Is_NoTSpecified_When_I_Call_UpdateDataRecordViaWebFormStatus_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_existingDataRecordMock.Object)).Returns(WebFormStatusType.NotProcessed);
            _blaiseApiServiceMock.Setup(b => b.GetWebFormStatus(_newDataRecordMock.Object)).Returns(WebFormStatusType.NotSpecified);

            //act
            _sut.UpdateDataRecordViaWebFormStatus(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetWebFormStatus(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateDataRecordByHoutServiceMock.VerifyNoOtherCalls();
        }

    }
}
