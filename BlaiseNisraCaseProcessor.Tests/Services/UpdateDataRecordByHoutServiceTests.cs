using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class UpdateDataRecordByHoutServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IPublishCaseStatusService> _publishCaseStatusServiceMock;

        private Mock<IDataRecord> _newDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;

        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private UpdateDataRecordByHoutService _sut;

        public UpdateDataRecordByHoutServiceTests()
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

            _publishCaseStatusServiceMock = new Mock<IPublishCaseStatusService>();

            _sut = new UpdateDataRecordByHoutService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _publishCaseStatusServiceMock.Object);
        }

        [Test]
        public void Given_The_HOut_Field_Does_Not_Exist_On_The_Nisra_File_When_I_Call_UpdateDataRecordByHoutValues_The_Existing_Record_Will_Not_Be_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(false);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.HOutFieldExists(_newDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_The_HOut_Field_On_The_Nisra_File_Is_Set_As_NotProcessed_When_I_Call_UpdateDataRecordByHoutValues_The_Existing_Record_Will_Not_Be_Updated()
        {
            //arrange
            var nisraHOutValue = 0;
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.HOutFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_newDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(2, 3)]
        public void Given_The_Nisra_HOut_Field_Is_Less_Than_The_Existing_HOut_Value_When_I_Call_UpdateDataRecordByHoutValues_The_Existing_Record_Is_Updated(int nisraHOutValue, int existingHOutValue)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.HOutFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.UpdateDataRecord(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName), Times.Once);


            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(2, 3)]
        public void Given_The_Nisra_HOut_Field_Is_Less_Than_The_Existing_HOut_Value_When_I_Call_UpdateDataRecordByHoutValues_Then_A_Message_Is_Published(int nisraHOutValue, int existingHOutValue)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_newDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        public void Given_Existing_HOut_Value_Is_Set_As_Not_Processed_But_The_Nisra_Is_When_I_Call_UpdateDataRecordByHoutValues_The_Existing_Record_Is_Updated(int nisraHOutValue, int existingHOutValue)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.HOutFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateDataRecord(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        public void Given_Existing_HOut_Value_Is_Set_As_Not_Processed_But_The_Nisra_Is_When_I_Call_UpdateDataRecordByHoutValues_Then_A_Message_Is_Published(int nisraHOutValue, int existingHOutValue)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_newDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(2, 1)]
        [TestCase(3, 2)]
        public void Given_Existing_HOut_Value_Is_Less_Then_The_Nisra_HOut_Value_When_I_Call_UpdateDataRecordByHoutValues_The_Existing_Record_Will_Not_Be_Updated(int nisraHOutValue, int existingHOutValue)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.HOutFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_newDataRecordMock.Object)).Returns(nisraHOutValue);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingHOutValue);

            //act
            _sut.UpdateDataRecordByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.HOutFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }
    }
}
