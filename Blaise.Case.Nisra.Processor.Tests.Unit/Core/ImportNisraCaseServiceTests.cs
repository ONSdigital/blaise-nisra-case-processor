using System;
using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Services;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.Core
{
    public class ImportNisraCaseServiceTests
    {
        private Mock<IBlaiseCaseApi> _blaiseApiMock;
        private Mock<ICatiDataService> _catiDataMock;
        private Mock<ILoggingService> _loggingMock;
        private MockSequence _mockSequence;

        private Mock<IDataRecord> _nisraDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;
        private Mock<IDataValue> _dataValueMock;

        private readonly string _primaryKey;
        private readonly string _serverParkName;
        private readonly string _instrumentName;
        private Dictionary<string, string> _newFieldData;
        private Dictionary<string, string> _existingFieldData;

        private ImportNisraCaseService _sut;

        public ImportNisraCaseServiceTests()
        {
            _primaryKey = "SN123";
            _serverParkName = "Park1";
            _instrumentName = "OPN123";
        }

        [SetUp]
        public void SetUpTests()
        {
            _blaiseApiMock = new Mock<IBlaiseCaseApi>();
            _loggingMock = new Mock<ILoggingService>();

            //set up new record
            _nisraDataRecordMock = new Mock<IDataRecord>();
            _newFieldData = new Dictionary<string, string>();
            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_nisraDataRecordMock.Object))
                .Returns(_newFieldData);

            //set up existing record
            _existingDataRecordMock = new Mock<IDataRecord>();
            _existingFieldData = new Dictionary<string, string>();
            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_existingDataRecordMock.Object))
                .Returns(_existingFieldData);

            //set up case not in use
            _dataValueMock = new Mock<IDataValue>();
            _dataValueMock.Setup(d => d.IntegerValue).Returns(0);
            _blaiseApiMock.Setup(b => b.CaseInUseInCati(_existingDataRecordMock.Object))
                .Returns(false);

            //important that the service calls the methods in the right order, otherwise you could end up removing what you have added
            _catiDataMock = new Mock<ICatiDataService>(MockBehavior.Strict);
            _mockSequence = new MockSequence();

            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCatiManaBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCallHistoryBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveWebNudgedField(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.AddCatiManaCallItems(_newFieldData, _existingFieldData,
                It.IsAny<int>()));

            //needed as we don't update if these fields match
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(DateTime.Now);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(DateTime.Now.AddHours(-1));

            _sut = new ImportNisraCaseService(
                _blaiseApiMock.Object,
                _catiDataMock.Object,
                _loggingMock.Object);
        }
        [Test]
        public void Given_A_New_dataRecord_When_I_Call_CreateOnlineCase_Then_The_Case_Is_Created()
        {
            //arrange
            const int outcomeCode = 110;
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(outcomeCode);

            //important that the service calls the methods in the right order, otherwise you could end up removing what you have added
            _catiDataMock = new Mock<ICatiDataService>(MockBehavior.Strict);
            _mockSequence = new MockSequence();

            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCatiManaBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.AddCatiManaCallItems(_newFieldData, _existingFieldData,
                It.IsAny<int>()));

            var sut = new ImportNisraCaseService(
                _blaiseApiMock.Object,
                _catiDataMock.Object,
                _loggingMock.Object);

            //act
            sut.CreateOnlineCase(_nisraDataRecordMock.Object, _instrumentName, _serverParkName, _primaryKey);

            //assert
            _catiDataMock.Verify(v => v.RemoveCatiManaBlock(_newFieldData), Times.Once);
            _catiDataMock.Verify(v => v.AddCatiManaCallItems(_newFieldData, _existingFieldData, outcomeCode),
                Times.Once);

            _blaiseApiMock.Verify(v => v.CreateCase(_primaryKey, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 1 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Complete_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated()
        {
            //arrange
            const int hOutComplete = 110; //complete

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName,
                _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.GetOutcomeCode(_nisraDataRecordMock.Object), Times.Once);
            _blaiseApiMock.Verify(v => v.GetOutcomeCode(_existingDataRecordMock.Object), Times.Once);

            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 2 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Has_An_Outcome_Of_Complete_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Not_Updated()
        {
            //arrange
            const int hOutPartial = 210; //partial
            const int hOutComplete = 110; //complete

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(It.IsAny<IDataRecord>(), It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // Scenario 3 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Has_An_Outcome_Of_Partial_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated()
        {
            //arrange
            const int hOutPartial = 210; //partial
            const int hOutComplete = 110; //complete

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutPartial);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 4  (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [TestCase(210)]
        [TestCase(310)]
        [TestCase(430)]
        [TestCase(460)]
        [TestCase(461)]
        [TestCase(541)]
        [TestCase(542)]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Has_An_Outcome_Between_210_And_542_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated(int existingOutcome)
        {
            //arrange
            const int hOutComplete = 110; //complete

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(existingOutcome);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 5 & 8 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [TestCase(110)]
        [TestCase(310)]
        public void Given_The_Nisra_Outcome_Is_Zero_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_Existing_Record_Is_Not_Updated(
            int existingOutcome)
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(0);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(existingOutcome);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName,
                _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(It.IsAny<IDataRecord>(), It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // Scenario 6 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_And_Existing_Case_Have_An_Outcome_Of_Partial_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated()
        {
            //arrange
            const int hOutPartial = 210; //partial

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutPartial);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 7 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [TestCase(210)]
        [TestCase(310)]
        [TestCase(430)]
        [TestCase(460)]
        [TestCase(461)]
        [TestCase(541)]
        [TestCase(542)]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Haw_An_Outcome_Between_210_And_542_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated(int existingOutcome)
        {
            //arrange
            const int hOutPartial = 210; //partial

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(existingOutcome);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 8 - covered by Scenario 5 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)

        //additional scenario
        [TestCase(110)]
        [TestCase(210)]
        public void Given_The_Nisra_Case_Has_A_Valid_Outcome_But_Existing_Case_Haw_An_Outcome_Of_Zero_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Updated(int nisraOutcome)
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(nisraOutcome);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(0);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        // Scenario 9 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Partial_And_Existing_Case_Has_An_Outcome_Of_Delete_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Not_Updated()
        {
            //arrange
            const int hOutPartial = 210; //partial
            const int hOutComplete = 562; //respondent request for data to be deleted

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(It.IsAny<IDataRecord>(), It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // Scenario 10 (https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow)
        [Test]
        public void Given_The_Nisra_Case_Has_An_Outcome_Of_Complete_And_Existing_Case_Has_An_Outcome_Of_Delete_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_To_Record_Is_Not_Updated()
        {
            //arrange
            const int hOutPartial = 110; //Complete
            const int hOutComplete = 561; //respondent request for data to be deleted

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutPartial);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(It.IsAny<IDataRecord>(), It.IsAny<Dictionary<string, string>>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // Scenario 11
        [Test]
        public void Given_Nisra_Is_Superior_But_The_Case_Is_In_Use_In_Cati_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_Case_Is_Not_Updated()
        {
            //arrange
            const int hOutComplete = 110; //complete
            const int hOutPartial = 210; //partial

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutPartial);

            //set up case is in use
            _blaiseApiMock.Setup(b => b.CaseInUseInCati(_existingDataRecordMock.Object))
                .Returns(true);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName,
                _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Never);
        }

        // Scenario 12
        [Test]
        public void Given_The_Case_Has_Been_Processed_Before_When_I_Call_UpdateExistingCaseWithOnlineData_Then_The_Case_Is_Not_Updated()
        {
            //arrange
            const int hOutComplete = 110; //complete

            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_nisraDataRecordMock.Object)).Returns(hOutComplete);
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(hOutComplete);

            //set up case already been processed
            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            //act
            _sut.UpdateExistingCaseWithOnlineData(_nisraDataRecordMock.Object, _existingDataRecordMock.Object, _serverParkName,
                _instrumentName, _primaryKey);

            //assert
            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Never);
        }

        [Test]
        public void Given_I_Call_UpdateCase_Then_The_Correct_Methods_Are_Called()
        {
            //arrange
            const int newOutcomeCode = 110;
            const int existingOutcomeCode = 210;

            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_nisraDataRecordMock.Object)).Returns(_newFieldData);
            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_existingDataRecordMock.Object)).Returns(_existingFieldData);
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCatiManaBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.AddCatiManaCallItems(_newFieldData, _existingFieldData,
                newOutcomeCode));
            _blaiseApiMock.Setup(b => b.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName));

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _instrumentName, _serverParkName, newOutcomeCode, existingOutcomeCode, _primaryKey);

            //assert
            _catiDataMock.Verify(v => v.RemoveCatiManaBlock(_newFieldData), Times.Once);
            _catiDataMock.Verify(v => v.RemoveCallHistoryBlock(_newFieldData), Times.Once);
            _catiDataMock.Verify(v => v.RemoveWebNudgedField(_newFieldData), Times.Once);
            _catiDataMock.Verify(v => v.AddCatiManaCallItems(_newFieldData, _existingFieldData, newOutcomeCode),
                Times.Once);

            _blaiseApiMock.Verify(v => v.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName), Times.Once);
        }

        [Test]
        public void Given_The_Record_Gets_Updated_When_I_Call_UpdateCase_Then_The_Update_Is_Logged()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetCase(_primaryKey, _instrumentName, _serverParkName))
                .Returns(_existingDataRecordMock.Object);

            const int outcomeCode = 110;
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(outcomeCode);

            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_nisraDataRecordMock.Object)).Returns(_newFieldData);
            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_existingDataRecordMock.Object)).Returns(_existingFieldData);
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCatiManaBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.AddCatiManaCallItems(_newFieldData, _existingFieldData,
                outcomeCode));
            _blaiseApiMock.Setup(b => b.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName));

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _instrumentName, _serverParkName, outcomeCode, outcomeCode, _primaryKey);

            //assert
            _loggingMock.Verify(v => v.LogInfo($"processed: NISRA case '{_primaryKey}' (NISRA HOut = '{outcomeCode}' <= '{outcomeCode}') or (Existing HOut = 0)'"),
                Times.Once);

            _loggingMock.Verify(v => v.LogWarn(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Given_The_Record_Does_Not_Get_Updated_When_I_Call_UpdateCase_Then_A_Warning_Is_Logged()
        {
            //arrange
            const int newOutcomeCode = 110;
            const int existingOutcomeCode = 210;

            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_nisraDataRecordMock.Object)).Returns(_newFieldData);
            _blaiseApiMock.Setup(b => b.GetRecordDataFields(_existingDataRecordMock.Object)).Returns(_existingFieldData);
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.RemoveCatiManaBlock(_newFieldData));
            _catiDataMock.InSequence(_mockSequence).Setup(c => c.AddCatiManaCallItems(_newFieldData, _existingFieldData,
                newOutcomeCode));
            _blaiseApiMock.Setup(b => b.UpdateCase(_existingDataRecordMock.Object, _newFieldData,
                _instrumentName, _serverParkName));

            //act
            _sut.UpdateCase(_nisraDataRecordMock.Object, _existingDataRecordMock.Object,
                _instrumentName, _serverParkName, newOutcomeCode, existingOutcomeCode, _primaryKey);

            //assert
            _loggingMock.Verify(v => v.LogWarn($"NISRA case '{_primaryKey}' failed to update - potentially open in Cati at the time of the update"),
                Times.Once);
        }

        [Test]
        public void Given_A_Record_Has_Updated_When_I_Call_RecordHasBeenUpdated_Then_True_Is_Returned()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetCase(_primaryKey, _instrumentName, _serverParkName))
                .Returns(_existingDataRecordMock.Object);

            const int outcomeCode = 110;
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(outcomeCode);

            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            //act
            var result = _sut.RecordHasBeenUpdated(_primaryKey, _nisraDataRecordMock.Object, outcomeCode, _instrumentName, _serverParkName);

            //assert
            Assert.IsNotNull(result);
            Assert.True(result);
        }

        [Test]
        public void Given_A_Record_Has_Not_Updated_Due_To_Different_OutComes_When_I_Call_RecordHasBeenUpdated_Then_False_Is_Returned()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetCase(_primaryKey, _instrumentName, _serverParkName))
                .Returns(_existingDataRecordMock.Object);

            const int existingOutcomeCode = 110;
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(existingOutcomeCode);

            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            //act
            var result = _sut.RecordHasBeenUpdated(_primaryKey, _nisraDataRecordMock.Object, 210, _instrumentName, _serverParkName);

            //assert
            Assert.IsNotNull(result);
            Assert.False(result);
        }

        [Test]
        public void Given_A_Record_Has_Not_Updated_Due_To_Different_LastUpdated_dates_When_I_Call_RecordHasBeenUpdated_Then_False_Is_Returned()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetCase(_primaryKey, _instrumentName, _serverParkName))
                .Returns(_existingDataRecordMock.Object);

            const int outcomeCode = 110;
            _blaiseApiMock.Setup(b => b.GetOutcomeCode(_existingDataRecordMock.Object)).Returns(outcomeCode);

            var nisraLastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(nisraLastUpdated);

            var existingLastUpdated = DateTime.Now.AddMinutes(-30);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(existingLastUpdated);

            //act
            var result = _sut.RecordHasBeenUpdated(_primaryKey, _nisraDataRecordMock.Object, outcomeCode, _instrumentName, _serverParkName);

            //assert
            Assert.IsNotNull(result);
            Assert.False(result);
        }

        [Test]
        public void Given_A_Record_Has_Been_Processed_Before_When_I_Call_NisraRecordHasAlreadyBeenProcessed_Then_True_Is_Returned()
        {
            //arrange
            const int nisraOutcomeCode = 110;
            const int existingOutcomeCode = 110;

            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            //act
            var result = _sut.NisraRecordHasAlreadyBeenProcessed(_nisraDataRecordMock.Object, nisraOutcomeCode,
                _existingDataRecordMock.Object, existingOutcomeCode);

            //assert
            Assert.IsNotNull(result);
            Assert.True(result);
        }

        [Test]
        public void Given_A_Record_Has_Not_Been_Processed_Before_Due_To_Different_Outcome_Codes_When_I_Call_NisraRecordHasAlreadyBeenProcessed_Then_False_Is_Returned()
        {
            //arrange
            const int nisraOutcomeCode = 110;
            const int existingOutcomeCode = 210;

            var lastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(lastUpdated);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(lastUpdated);

            //act
            var result = _sut.NisraRecordHasAlreadyBeenProcessed(_nisraDataRecordMock.Object, nisraOutcomeCode,
                _existingDataRecordMock.Object, existingOutcomeCode);

            //assert
            Assert.IsNotNull(result);
            Assert.False(result);
        }

        [Test]
        public void Given_A_Record_Has_Not_Been_Processed_Before_Due_To_Different_LastUpdated_Dates_When_I_Call_NisraRecordHasAlreadyBeenProcessed_Then_False_Is_Returned()
        {
            //arrange
            const int nisraOutcomeCode = 110;
            const int existingOutcomeCode = 110;

            var nisraLastUpdated = DateTime.Now;
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_nisraDataRecordMock.Object)).Returns(nisraLastUpdated);

            var existingLastUpdated = DateTime.Now.AddMinutes(-30);
            _blaiseApiMock.Setup(b => b.GetLastUpdatedDateTime(_existingDataRecordMock.Object)).Returns(existingLastUpdated);

            //act
            var result = _sut.NisraRecordHasAlreadyBeenProcessed(_nisraDataRecordMock.Object, nisraOutcomeCode,
                _existingDataRecordMock.Object, existingOutcomeCode);

            //assert
            Assert.IsNotNull(result);
            Assert.False(result);
        }
    }
}
