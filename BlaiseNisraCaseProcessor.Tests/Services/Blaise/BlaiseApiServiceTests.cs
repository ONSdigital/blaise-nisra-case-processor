using System.Collections.Generic;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.Api.Contracts.Models;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Services.Blaise;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Tests.Services.Blaise
{
    public class BlaiseApiServiceTests
    {
        private Mock<IFluentBlaiseApi> _blaiseApiMock;
        private Mock<ICaseMapper> _mapperMock;

        private readonly ConnectionModel _connectionModel;
        private readonly string _serialNumber;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private BlaiseApiService _sut;

        public BlaiseApiServiceTests()
        {
            _connectionModel = new ConnectionModel();
            _serialNumber = "SN123";
            _serverParkName = "Park1";
            _surveyName = "OPN123";
        }

        [SetUp]
        public void SetUpTests()
        {
            _blaiseApiMock = new Mock<IFluentBlaiseApi>();
            _blaiseApiMock.Setup(b => b.DefaultConnection).Returns(_connectionModel);

            _mapperMock = new Mock<ICaseMapper>();

            _sut = new BlaiseApiService(
                _blaiseApiMock.Object,
                _mapperMock.Object);
        }

        [Test]
        public void Given_I_Call_GetAvailableServerParks_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b.WithConnection(_connectionModel).ServerParks).Returns(new List<string>());

            //act
            _sut.GetAvailableServerParks();

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v.WithConnection(_connectionModel).ServerParks, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetAvailableServerParks_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var serverParks = new List<string>{"Park1", "Park2"};
            _blaiseApiMock.Setup(b => b.WithConnection(_connectionModel).ServerParks).Returns(serverParks);

            //act
            var result = _sut.GetAvailableServerParks();

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(serverParks, result);
        }

        [Test]
        public void Given_I_Call_CaseExists_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Exists).Returns(true);

            //act
            _sut.CaseExists(_serialNumber, _serverParkName, _surveyName);

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Exists, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_I_Call_CaseExists_Then_The_Expected_Value_Is_Returned(bool exists)
        {
            //arrange
            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Exists).Returns(exists);

            //act
            var result = _sut.CaseExists(_serialNumber, _serverParkName, _surveyName);

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

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithFile(databaseFile)
                .Cases).Returns(dataSetMock.Object);

            //act
            _sut.GetCasesFromFile(databaseFile);

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v
                .WithConnection(_connectionModel)
                .WithFile(databaseFile)
                .Cases, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetCasesFromFile_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var databaseFile = "File1";
            var dataSetMock = new Mock<IDataSet>();

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithFile(databaseFile)
                .Cases).Returns(dataSetMock.Object);

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
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .Add());

            //act
            _sut.AddDataRecord(dataRecordMock.Object, _serverParkName, _surveyName);

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .Add(), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetDataRecord_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Get());

            //act
            _sut.GetDataRecord(_serialNumber, _serverParkName, _surveyName);

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Get(), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_GetDataRecord_Then_The_Expected_Value_Is_Returned()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithPrimaryKey(_serialNumber)
                .Get()).Returns(dataRecordMock.Object);

            //act
            var result = _sut.GetDataRecord(_serialNumber, _serverParkName, _surveyName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreSame(dataRecordMock.Object, result);
        }

        [Test]
        public void Given_I_Call_WebFormStatusFieldExists_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.WebFormStatus));

            //act
            _sut.WebFormStatusFieldExists(dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.WebFormStatus), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_I_Call_WebFormStatusFieldExists_Then_The_Expected_Value_Is_Returned(bool exists)
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.WebFormStatus)).Returns(exists);

            //act
            var result = _sut.WebFormStatusFieldExists(dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_I_Call_HOutFieldExists_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.HOut));

            //act
            _sut.HOutFieldExists(dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.HOut), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_I_Call_HOutFieldExists_Then_The_Expected_Value_Is_Returned(bool exists)
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HasField(FieldNameType.HOut)).Returns(exists);

            //act
            var result = _sut.HOutFieldExists(dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_I_Call_GetWebFormStatus_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .WebFormStatus);

            //act
            _sut.GetWebFormStatus(dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .WebFormStatus, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(WebFormStatusType.Complete)]
        [TestCase(WebFormStatusType.NotProcessed)]
        [TestCase(WebFormStatusType.Partial)]
        [TestCase(WebFormStatusType.NotSpecified)]
        public void Given_I_Call_GetWebFormStatus_Then_The_Expected_Value_Is_Returned(WebFormStatusType expectedResult)
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .WebFormStatus).Returns(expectedResult);

            //act
            var result = _sut.GetWebFormStatus(dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Given_I_Call_GetHOutValue_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HOut);

            //act
            _sut.GetHOutValue(dataRecordMock.Object);

            //assert
            _blaiseApiMock.Verify(v => v
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HOut, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Given_I_Call_GetHOutValue_Then_The_Expected_Value_Is_Returned(decimal expectedResult)
        {
            //arrange
            var dataRecordMock = new Mock<IDataRecord>();

            _blaiseApiMock.Setup(b => b
                .Case
                .WithDataRecord(dataRecordMock.Object)
                .HOut).Returns(expectedResult);

            //act
            var result = _sut.GetHOutValue(dataRecordMock.Object);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Given_I_Call_UpdateDataRecord_Then_The_Correct_Service_Methods_Are_Called()
        {
            //arrange
            var newDataRecordMock = new Mock<IDataRecord2>();
            var existingDataRecordMock = new Mock<IDataRecord>();

            var fieldData = new Dictionary<string, string>();

            _mapperMock.Setup(m => m.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object)).Returns(fieldData);

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithDataRecord(existingDataRecordMock.Object)
                .WithData(fieldData)
                .Update());

            //act
            _sut.UpdateDataRecord(newDataRecordMock.Object, existingDataRecordMock.Object, _serverParkName, _surveyName);

            //assert
            _blaiseApiMock.Verify(v => v.DefaultConnection, Times.Once);
            _blaiseApiMock.Verify(v => v
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithDataRecord(existingDataRecordMock.Object)
                .WithData(fieldData)
                .Update(), Times.Once);

            _mapperMock.Verify(v => v.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_I_Call_UpdateDataRecord_Then_The_OnLine_Flag_Is_Added_To_The_FieldData_Dictionary()
        {
            //arrange
            var newDataRecordMock = new Mock<IDataRecord2>();
            var existingDataRecordMock = new Mock<IDataRecord>();

            var fieldData = new Dictionary<string, string>();

            _mapperMock.Setup(m => m.MapFieldDictionaryFromRecordFields(newDataRecordMock.Object)).Returns(fieldData);

            _blaiseApiMock.Setup(b => b
                .WithConnection(_connectionModel)
                .WithServerPark(_serverParkName)
                .WithInstrument(_surveyName)
                .Case
                .WithDataRecord(existingDataRecordMock.Object)
                .WithData(fieldData)
                .Update());

            //act
            _sut.UpdateDataRecord(newDataRecordMock.Object, existingDataRecordMock.Object, _serverParkName, _surveyName);

            //assert
            Assert.IsTrue(fieldData.ContainsKey("QHAdmin.Online"));
            Assert.AreEqual("1", fieldData["QHAdmin.Online"]);
        }
    }
}
