﻿using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class ImportFileServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IUpdateCaseByHoutService> _updateByHoutServiceMock;
        private Mock<IUpdateCaseServiceService> _updateByWebFormStatusMock;

        private Mock<IDataRecord> _newDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;
        private Mock<IDataSet> _dataSetMock;

        private readonly string _serialNumber;
        private readonly string _databaseFileName;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private ImportFileService _sut;

        public ImportFileServiceTests()
        {
            _serialNumber = "SN123";
            _serverParkName = "Park1";
            _surveyName = "OPN123";
            _databaseFileName = "OPN123.bdbx";
        }

        [SetUp]
        public void SetUpTests()
        {
            _newDataRecordMock = new Mock<IDataRecord>();
            _existingDataRecordMock = new Mock<IDataRecord>();

            _dataSetMock = new Mock<IDataSet>();
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);

            _loggingMock = new Mock<ILog>();

            _blaiseApiServiceMock = new Mock<IBlaiseApiService>();
            _blaiseApiServiceMock.Setup(b => b.GetCasesFromFile(_databaseFileName)).Returns(_dataSetMock.Object);

            _updateByHoutServiceMock = new Mock<IUpdateCaseByHoutService>();

            _updateByWebFormStatusMock = new Mock<IUpdateCaseServiceService>();

            _sut = new ImportFileService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _updateByHoutServiceMock.Object,
                _updateByWebFormStatusMock.Object);
        }

        [Test]
        public void Given_There_Are_No_Records_Available_In_The_Nisra_File_When_I_Call_ImportSurveyRecordsFromFile_Then_Nothing_Is_Processed()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(true);

            //act
            _sut.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetCasesFromFile(_databaseFileName), Times.Once);
            _dataSetMock.Verify(v => v.EndOfSet, Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateByHoutServiceMock.VerifyNoOtherCalls();
            _updateByWebFormStatusMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_A_Record_In_The_Nisra_File_Does_Not_Exist_When_I_Call_ImportSurveyRecordsFromFile_Then_The_Record_Is_Added()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetSerialNumber(_newDataRecordMock.Object)).Returns(_serialNumber);
            _blaiseApiServiceMock.Setup(b => b.CaseExists(_serialNumber, _serverParkName, _surveyName)).Returns(false);

            //act
            _sut.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetCasesFromFile(_databaseFileName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetSerialNumber(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.CaseExists(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.AddDataRecord(_newDataRecordMock.Object, _serverParkName, _surveyName), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);


            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateByHoutServiceMock.VerifyNoOtherCalls();
            _updateByWebFormStatusMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_A_Record_In_The_Nisra_File_That_Exists_And_The_WebStatusField_Exists_On_Both_Records_When_I_Call_ImportSurveyRecordsFromFile_Then_The_Record_Is_Updated_By_WebStatus()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetSerialNumber(_newDataRecordMock.Object)).Returns(_serialNumber);
            _blaiseApiServiceMock.Setup(b => b.CaseExists(_serialNumber, _serverParkName, _surveyName)).Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetDataRecord(_serialNumber, _serverParkName, _surveyName))
                .Returns(_existingDataRecordMock.Object);

            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_existingDataRecordMock.Object)).Returns(true);

            //act
            _sut.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetCasesFromFile(_databaseFileName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetSerialNumber(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.CaseExists(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetDataRecord(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.WebFormStatusFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.WebFormStatusFieldExists(_existingDataRecordMock.Object), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);

            _updateByWebFormStatusMock.Verify(v => v.UpdateCase(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateByHoutServiceMock.VerifyNoOtherCalls();
            _updateByWebFormStatusMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_A_Record_In_The_Nisra_File_That_Exists_And_The_WebStatusFieldExists_Only_On_Nisra_File_When_I_Call_ImportSurveyRecordsFromFile_Then_The_Record_Is_Updated_By_HOut()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetSerialNumber(_newDataRecordMock.Object)).Returns(_serialNumber);
            _blaiseApiServiceMock.Setup(b => b.CaseExists(_serialNumber, _serverParkName, _surveyName)).Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetDataRecord(_serialNumber, _serverParkName, _surveyName))
                .Returns(_existingDataRecordMock.Object);

            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_newDataRecordMock.Object)).Returns(true);
            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_existingDataRecordMock.Object)).Returns(false);

            //act
            _sut.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetCasesFromFile(_databaseFileName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetSerialNumber(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.CaseExists(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetDataRecord(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.WebFormStatusFieldExists(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.WebFormStatusFieldExists(_existingDataRecordMock.Object), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);

            _updateByHoutServiceMock.Verify(v => v.UpdateCaseByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateByHoutServiceMock.VerifyNoOtherCalls();
            _updateByWebFormStatusMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_A_Record_In_The_Nisra_File_That_Exists_And_The_WebStatusFieldExists_Only_On_Existing_File_When_I_Call_ImportSurveyRecordsFromFile_Then_The_Record_Is_Updated_By_HOut()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetSerialNumber(_newDataRecordMock.Object)).Returns(_serialNumber);
            _blaiseApiServiceMock.Setup(b => b.CaseExists(_serialNumber, _serverParkName, _surveyName)).Returns(true);

            _blaiseApiServiceMock.Setup(b => b.GetDataRecord(_serialNumber, _serverParkName, _surveyName))
                .Returns(_existingDataRecordMock.Object);

            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_newDataRecordMock.Object)).Returns(false);
            _blaiseApiServiceMock.Setup(b => b.WebFormStatusFieldExists(_existingDataRecordMock.Object)).Returns(true);

            //act
            _sut.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetCasesFromFile(_databaseFileName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetSerialNumber(_newDataRecordMock.Object), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.CaseExists(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetDataRecord(_serialNumber, _serverParkName, _surveyName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.WebFormStatusFieldExists(_newDataRecordMock.Object), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);

            _updateByHoutServiceMock.Verify(v => v.UpdateCaseByHoutValues(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _surveyName, _serialNumber), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _updateByHoutServiceMock.VerifyNoOtherCalls();
            _updateByWebFormStatusMock.VerifyNoOtherCalls();
        }
    }
}
