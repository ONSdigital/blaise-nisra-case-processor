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
        private Mock<IPublishCaseStatusService> _publishCaseStatusServiceMock;

        private Mock<IDataRecord> _nisraDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;

        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private UpdateCaseService _sut;

        public UpdateCaseServiceTests()
        {
            _serialNumber = "SN123";
            _serverParkName = "Park1";
            _surveyName = "OPN123";
        }

        [SetUp]
        public void SetUpTests()
        {
            _nisraDataRecordMock = new Mock<IDataRecord>();
            _existingDataRecordMock = new Mock<IDataRecord>();

            _loggingMock = new Mock<ILog>();

            _blaiseApiServiceMock = new Mock<IBlaiseApiService>();

            _publishCaseStatusServiceMock = new Mock<IPublishCaseStatusService>();

            _sut = new UpdateCaseService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _publishCaseStatusServiceMock.Object);
        }

        // Scenario 1 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Complete_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated()
        {
            //arrange
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Complete_When_I_Call_UpdateCase_Then_A_Message_Is_Published()
        {
            //arrange
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_nisraDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 2 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Has_An_Outcome_Of_Complete_When_I_Call_UpdateCase_Then_The_To_Record_Is_Not_Updated()
        {
            //arrange
            var hOutPartial = 210; //partial
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 3 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Has_An_Outcome_Of_Partial_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated()
        {
            //arrange
            var hOutPartial = 210; //partial
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutPartial);
            
            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Has_An_Outcome_Of_Partial_When_I_Call_UpdateCase_Then_Then_A_Message_Is_Published()
        {
            //arrange
            var hOutPartial = 210; //partial
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutPartial);


            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_nisraDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 4  (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [TestCase(210)]
        [TestCase(300)]
        [TestCase(400)]
        [TestCase(500)]
        [TestCase(542)]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Haw_An_Outcome_Between_210_And_542_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated(int existingOutcome)
        {
            //arrange
            var hOutComplete = 110; //complete

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingOutcome);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 5 & 8 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Status_Is_NotProcessed_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(0);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_The_Nisra_Outcome_Is_Zero_When_I_Call_UpdateCase_Then_The_Existing_Record_Is_Not_Updated()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(0);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 6 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Partial_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated()
        {
            //arrange
            var hOutPartial = 210; //partial

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutPartial);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();

        }

        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Partial_When_I_Call_UpdateCase_Then_A_Message_Is_Published()
        {
            //arrange
            var hOutPartial = 210; //partial

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(hOutPartial);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_nisraDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 7 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [TestCase(310)]
        [TestCase(300)]
        [TestCase(400)]
        [TestCase(500)]
        [TestCase(542)]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Haw_An_Outcome_Between_210_And_542_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated(int existingOutcome)
        {
            //arrange
            var hOutPartial = 210; //partial

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingOutcome);

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(310)]
        [TestCase(300)]
        [TestCase(400)]
        [TestCase(500)]
        [TestCase(542)]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Haw_An_Outcome_Between_210_And_542_When_I_Call_UpdateCase_Then_Then_A_Message_Is_Published(int existingOutcome)
        {
            //arrange
            var hOutPartial = 210; //partial

            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(existingOutcome);


            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_nisraDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }

        // Scenario 8 - covered by Scenario 5 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)

        //additional scenario
        [TestCase(110)]
        [TestCase(210)]
        public void Given_The_Nisra_Case_Has_A_Valid_Outcome_But_Existing_Case_Haw_An_Outcome_Of_Zero_When_I_Call_UpdateCase_Then_The_To_Record_Is_Updated(int nisraOutcome)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(nisraOutcome);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(0);


            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetHOutValue(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiServiceMock.Verify(v => v.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName));

            _blaiseApiServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(110)]
        [TestCase(210)]
        public void Given_The_Nisra_Case_Has_A_Valid_Outcome_But_Existing_Case_Haw_An_Outcome_Of_Zero_When_I_Call_UpdateCase_Then_Then_A_Message_Is_Published(int nisraOutcome)
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_nisraDataRecordMock.Object)).Returns(nisraOutcome);
            _blaiseApiServiceMock.Setup(b => b.GetHOutValue(_existingDataRecordMock.Object)).Returns(0);


            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _surveyName, _serialNumber);

            //assert
            _publishCaseStatusServiceMock.Verify(v => v.PublishCaseStatus(_nisraDataRecordMock.Object,
                _surveyName, _serverParkName, CaseStatusType.NisraCaseImported), Times.Once);

            _publishCaseStatusServiceMock.VerifyNoOtherCalls();
        }
    }
}