using System.Collections.Generic;
using Blaise.Case.Nisra.Processor.Data;
using Blaise.Case.Nisra.Processor.Data.Interfaces;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.Api.Contracts.Models;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.Services
{
    public class BlaiseApiServiceTests
    {
        private Mock<IBlaiseApi> _blaiseApiMock;
        private Mock<IDataRecordMapper> _mapperMock;

        private Mock<IDataRecord> _dataRecordMock;

        private readonly ConnectionModel _connectionModel;
        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _instrumentName;

        private BlaiseApiService _sut;

        public BlaiseApiServiceTests()
        {
            _connectionModel = new ConnectionModel();
            _serialNumber = "SN123";
            _serverParkName = "Park1";
            _instrumentName = "OPN123";
        }

        [SetUp]
        public void SetUpTests()
        {
            _dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock = new Mock<IBlaiseApi>();
            _blaiseApiMock.Setup(b => b.GetDefaultConnectionModel()).Returns(_connectionModel);

            _mapperMock = new Mock<IDataRecordMapper>();

            _sut = new BlaiseApiService(
                _blaiseApiMock.Object,
                _mapperMock.Object);
        }

        [Test]
        public void Given_I_Call_GetAvailableServerParks_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetServerParkNames(_connectionModel)).Returns(new List<string>());

            //act
            _sut.GetAvailableServerParks();

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.GetServerParkNames(_connectionModel), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetAvailableServerParks_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var serverParks = new List<string> { "Park1", "Park2" };
            _blaiseApiMock.Setup(b => b.GetServerParkNames(_connectionModel)).Returns(serverParks);

            //act
            var result = _sut.GetAvailableServerParks();

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(serverParks, result);
        }

        [Test]
        public void Given_I_Call_SurveyExists_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.SurveyExists(_connectionModel, _instrumentName, _serverParkName)).Returns(true);

            //act
            _sut.SurveyExists(_serverParkName, _instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.SurveyExists(_connectionModel, _instrumentName, _serverParkName), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_I_Call_SurveyExists_Then_The_Expected_Value_Is_Returned(bool exists)
        {
            //arrange
            _blaiseApiMock.Setup(b => b.SurveyExists(_connectionModel, _instrumentName, _serverParkName)).Returns(exists);

            //act
            var result = _sut.SurveyExists(_serverParkName, _instrumentName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_I_Call_GetSerialNumber_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetPrimaryKeyValue(_dataRecordMock.Object)).Returns(It.IsAny<string>());

            //act
            _sut.GetSerialNumber(_dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v.GetPrimaryKeyValue(_dataRecordMock.Object), Times.Once);
            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_CaseExists_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var serialNumber = "10001";

            _blaiseApiMock.Setup(b => b.GetPrimaryKeyValue(_dataRecordMock.Object)).Returns(serialNumber);

            //act
            var result = _sut.GetSerialNumber(_dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(serialNumber, result);
        }

        [Test]
        public void Given_I_Call_CaseExists_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.CaseExists(_connectionModel, _serialNumber, _instrumentName, _serverParkName)).Returns(true);

            //act
            _sut.CaseExists(_serialNumber, _serverParkName, _instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.CaseExists(_connectionModel, _serialNumber, _instrumentName, _serverParkName), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_I_Call_CaseExists_Then_The_Expected_Value_Is_Returned(bool exists)
        {
            //arrange
            _blaiseApiMock.Setup(b => b.CaseExists(_connectionModel, _serialNumber, _instrumentName, _serverParkName)).Returns(exists);

            //act
            var result = _sut.CaseExists(_serialNumber, _serverParkName, _instrumentName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_I_Call_GetCasesFromFile_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var databaseFile = "File1";
            var dataSetMock = new Mock<IDataSet>();

            _blaiseApiMock.Setup(b => b.GetDataSet(databaseFile)).Returns(dataSetMock.Object);

            //act
            _sut.GetCasesFromFile(databaseFile);

            //assert
            _blaiseApiMock.Verify(v => v.GetDataSet(databaseFile), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetCasesFromFile_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var databaseFile = "File1";
            var dataSetMock = new Mock<IDataSet>();

            _blaiseApiMock.Setup(b => b.GetDataSet(databaseFile)).Returns(dataSetMock.Object);

            //act
            var result = _sut.GetCasesFromFile(databaseFile);

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(dataSetMock.Object, result);
        }

        [Test]
        public void Given_I_Call_AddDataRecord_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord2>();

            var fieldData = new Dictionary<string, string>();

            _mapperMock.Setup(m => m.MapFieldDictionaryFromRecordFields(dataRecordMock.Object)).Returns(fieldData);
            _blaiseApiMock.Setup(b => b.CreateNewDataRecord(_connectionModel, _serialNumber, fieldData, _instrumentName, _serverParkName));

            //act
            _sut.AddDataRecord(dataRecordMock.Object, _serialNumber, _serverParkName, _instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.CreateNewDataRecord(_connectionModel, _serialNumber, fieldData, _instrumentName, _serverParkName), Times.Once);
            _blaiseApiMock.VerifyNoOtherCalls();

            _mapperMock.Verify(v => v.MapFieldDictionaryFromRecordFields(dataRecordMock.Object), Times.Once);
        }

        [Test]
        public void Given_I_Call_GetDataRecord_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.GetDataRecord(_connectionModel, _serialNumber, _instrumentName, _serverParkName));

            //act
            _sut.GetDataRecord(_serialNumber, _serverParkName, _instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.GetDataRecord(_connectionModel, _serialNumber, _instrumentName, _serverParkName), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetDataRecord_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b.GetDataRecord(_connectionModel, _serialNumber, _instrumentName, _serverParkName)).Returns(dataRecordMock.Object);

            //act
            var result = _sut.GetDataRecord(_serialNumber, _serverParkName, _instrumentName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(dataRecordMock.Object, result);
        }

        [Test]
        public void Given_I_Call_GetHOutValue_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataValueMock = new Mock<IDataValue>();
            dataValueMock.Setup(d => d.IntegerValue).Returns(110);

            _blaiseApiMock.Setup(b => b.GetFieldValue(It.IsAny<IDataRecord>(), It.IsAny<FieldNameType>())).Returns(dataValueMock.Object);

            //act
            _sut.GetHOutValue(_dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v.GetFieldValue(_dataRecordMock.Object, FieldNameType.HOut), Times.Once);
        }

        [TestCase(110)]
        [TestCase(210)]
        public void Given_I_Call_GetHOutValue_Then_The_Expected_Value_Is_Returned(decimal expectedResult)
        {
            //arrange
            var dataValueMock = new Mock<IDataValue>();
            dataValueMock.Setup(d => d.IntegerValue).Returns(expectedResult);

            _blaiseApiMock.Setup(b => b.GetFieldValue(It.IsAny<IDataRecord>(), It.IsAny<FieldNameType>())).Returns(dataValueMock.Object);

            //act
            var result = _sut.GetHOutValue(_dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Given_I_Call_UpdateCase_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var newDataRecordMock = new Mock<IDataRecord2>();
            var existingDataRecordMock = new Mock<IDataRecord>();

            var fieldData = new Dictionary<string, string>();

            _mapperMock.Setup(m => m.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object)).Returns(fieldData);

            _blaiseApiMock.Setup(b => b.UpdateDataRecord(_connectionModel, existingDataRecordMock.Object, fieldData, _instrumentName, _serverParkName));

            //act
            _sut.UpdateCase(newDataRecordMock.Object, existingDataRecordMock.Object, _serverParkName, _instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.GetDefaultConnectionModel(), Times.Once);
            _blaiseApiMock.Verify(v => v.UpdateDataRecord(_connectionModel, existingDataRecordMock.Object, fieldData, _instrumentName, _serverParkName), Times.Once);
            _blaiseApiMock.VerifyNoOtherCalls();

            _mapperMock.Verify(v => v.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object), Times.Once);
        }

        [Test]
        public void Given_I_Call_UpdateCase_Then_The_OnLine_Flag_Is_Added_To_The_FieldData_Dictionary()
        {
            //arrange
            var newDataRecordMock = new Mock<IDataRecord2>();
            var existingDataRecordMock = new Mock<IDataRecord>();

            var fieldData = new Dictionary<string, string>();

            _mapperMock.Setup(m => m.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object)).Returns(fieldData);
            _blaiseApiMock.Setup(b => b.UpdateDataRecord(_connectionModel, existingDataRecordMock.Object, fieldData, _instrumentName, _serverParkName));

            //act
            _sut.UpdateCase(newDataRecordMock.Object, existingDataRecordMock.Object, _serverParkName, _instrumentName);

            //assert
            Assert.IsTrue(fieldData.ContainsKey("QHAdmin.Online"));
            Assert.AreEqual("1", fieldData["QHAdmin.Online"]);
        }
    }
}
