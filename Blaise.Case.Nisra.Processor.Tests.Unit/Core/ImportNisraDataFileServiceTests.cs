using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Services;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.DataLink;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.Core
{
    public class ImportNisraDataFileServiceTests
    {
        private Mock<IBlaiseCaseApi> _blaiseApiMock;
        private Mock<INisraCaseService> _nisraCaseServiceMock;
        private Mock<ILoggingService> _loggingMock;

        private Mock<IDataRecord> _newDataRecordMock;
        private Mock<IDataRecord> _existingDataRecordMock;
        private Mock<IDataSet> _dataSetMock;

        private readonly string _primaryKey;
        private readonly string _databaseFileName;
        private readonly string _serverParkName;
        private readonly string _instrumentName;

        private ImportNisraDataFileService _sut;

        public ImportNisraDataFileServiceTests()
        {
            _primaryKey = "SN123";
            _serverParkName = "Park1";
            _instrumentName = "OPN123";
            _databaseFileName = "OPN123.bdbx";
        }

        [SetUp]
        public void SetUpTests()
        {
            _newDataRecordMock = new Mock<IDataRecord>();
            _existingDataRecordMock = new Mock<IDataRecord>();

            _dataSetMock = new Mock<IDataSet>();
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);

            _blaiseApiMock = new Mock<IBlaiseCaseApi>();
            _blaiseApiMock.Setup(b => b.GetCases(_databaseFileName)).Returns(_dataSetMock.Object);

            _nisraCaseServiceMock = new Mock<INisraCaseService>();
            _loggingMock = new Mock<ILoggingService>();

            _sut = new ImportNisraDataFileService(
                _blaiseApiMock.Object,
                _nisraCaseServiceMock.Object,
                _loggingMock.Object);
        }

        [Test]
        public void Given_There_Are_No_Records_Available_In_The_Nisra_File_When_I_Call_ImportOnlineDatabaseFile_Then_Nothing_Is_Processed()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(true);

            //act
            _sut.ImportNisraDatabaseFile(_serverParkName, _instrumentName, _databaseFileName);

            //assert
            _blaiseApiMock.Verify(v => v.GetCases(_databaseFileName), Times.Once);
            _dataSetMock.Verify(v => v.EndOfSet, Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
            _nisraCaseServiceMock.VerifyNoOtherCalls();
            _nisraCaseServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_A_Record_In_The_Nisra_File_Does_Not_Exist_When_I_Call_ImportOnlineDatabaseFile_Then_The_Record_Is_Added()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiMock.Setup(b => b.GetPrimaryKeyValue(_newDataRecordMock.Object)).Returns(_primaryKey);
            _blaiseApiMock.Setup(b => b.CaseExists(_primaryKey, _serverParkName, _instrumentName)).Returns(false);

            //act
            _sut.ImportNisraDatabaseFile(_serverParkName, _instrumentName, _databaseFileName);

            //assert
            _blaiseApiMock.Verify(v => v.GetCases(_databaseFileName), Times.Once);
            _blaiseApiMock.Verify(v => v.GetPrimaryKeyValue(_newDataRecordMock.Object), Times.Once);
            _blaiseApiMock.Verify(v => v.CaseExists(_primaryKey, _instrumentName, _serverParkName), Times.Once);

            _nisraCaseServiceMock.Verify(v => v.CreateNisraCase(_newDataRecordMock.Object, _instrumentName,
                _serverParkName, _primaryKey), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);
            
            _blaiseApiMock.VerifyNoOtherCalls();
            _nisraCaseServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_A_Record_Already_Exists_When_I_Call_ImportCasesFromFile_Then_The_Record_Is_Updated()
        {
            //arrange
            _dataSetMock.Setup(d => d.ActiveRecord).Returns(_newDataRecordMock.Object);
            _dataSetMock.SetupSequence(d => d.EndOfSet)
                .Returns(false)
                .Returns(true);

            _blaiseApiMock.Setup(b => b.GetPrimaryKeyValue(_newDataRecordMock.Object)).Returns(_primaryKey);
            _blaiseApiMock.Setup(b => b.CaseExists(_primaryKey, _instrumentName, _serverParkName)).Returns(true);

            _blaiseApiMock.Setup(b => b.GetCase(_primaryKey, _instrumentName, _serverParkName))
                .Returns(_existingDataRecordMock.Object);

            //act
            _sut.ImportNisraDatabaseFile(_serverParkName, _instrumentName, _databaseFileName);

            //assert
            _blaiseApiMock.Verify(v => v.GetCases(_databaseFileName), Times.Once);
            _blaiseApiMock.Verify(v => v.GetPrimaryKeyValue(_newDataRecordMock.Object), Times.Once);
            _blaiseApiMock.Verify(v => v.CaseExists(_primaryKey, _instrumentName, _serverParkName), Times.Once);
            _blaiseApiMock.Verify(v => v.GetCase(_primaryKey, _instrumentName, _serverParkName), Times.Once);

            _dataSetMock.Verify(v => v.EndOfSet, Times.Exactly(2));
            _dataSetMock.Verify(v => v.ActiveRecord, Times.Once);
            _dataSetMock.Verify(v => v.MoveNext(), Times.Once);

            _nisraCaseServiceMock.Verify(v => v.ImportNisraCase(_newDataRecordMock.Object, _existingDataRecordMock.Object,
                _serverParkName, _instrumentName, _primaryKey), Times.Once);

            _blaiseApiMock.VerifyNoOtherCalls();
            _nisraCaseServiceMock.VerifyNoOtherCalls();
        }
    }
}
