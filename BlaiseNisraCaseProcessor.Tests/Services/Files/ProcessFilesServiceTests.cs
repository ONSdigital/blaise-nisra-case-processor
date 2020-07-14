using System.Collections.Generic;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Interfaces.Services.Blaise;
using BlaiseNisraCaseProcessor.Interfaces.Services.Files;
using BlaiseNisraCaseProcessor.Services.Files;
using log4net;
using Moq;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Services.Files
{
    public class ProcessFilesServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IFileService> _fileServiceMock;
        private Mock<IImportFileService> _importFileServiceMock;

        private readonly List<string> _availableFiles;
        private readonly List<string> _databaseFiles;
        private readonly List<string> _serverParks;
        private readonly string _databaseFileName;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private ProcessFilesService _sut;

        public ProcessFilesServiceTests()
        {
            _serverParkName = "Park1";
            _surveyName = "OPN123";
            _databaseFileName = "OPN123.bdbx";

            _availableFiles = new List<string> { "OPN123.bdbx", "OPN123.bdix" };
            _databaseFiles = new List<string> { _databaseFileName };
            _serverParks = new List<string> { _serverParkName };
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();

            _blaiseApiServiceMock = new Mock<IBlaiseApiService>();
            _blaiseApiServiceMock.Setup(b => b.GetAvailableServerParks()).Returns(_serverParks);
            _blaiseApiServiceMock.Setup(b => b.SurveyExists(_serverParkName, _surveyName)).Returns(true);

            _fileServiceMock = new Mock<IFileService>();
            _fileServiceMock.Setup(f => f.GetDatabaseFilesAvailable(_availableFiles)).Returns(_databaseFiles);
            _fileServiceMock.Setup(f => f.GetSurveyNameFromFile(_databaseFileName)).Returns(_surveyName);

            _importFileServiceMock = new Mock<IImportFileService>();

            _sut = new ProcessFilesService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _fileServiceMock.Object,
                _importFileServiceMock.Object);
        }

        [Test]
        public void Given_No_Database_Files_Are_In_The_Files_List_When_I_Call_ProcessFiles_Then_Nothing_Is_Processed()
        {
            //arrange
            _fileServiceMock.Setup(f => f.GetDatabaseFilesAvailable(It.IsAny<List<string>>()))
                .Returns(new List<string>());

            //act
            _sut.ProcessFiles(_availableFiles);

            //assert
            _fileServiceMock.Verify(v => v.GetDatabaseFilesAvailable(_availableFiles), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _fileServiceMock.VerifyNoOtherCalls();
            _importFileServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_DataBaseFiles_Available_But_No_ServerParks_When_I_Call_ProcessFiles_Then_Nothing_Is_Processed()
        {
            //arrange
            _blaiseApiServiceMock.Setup(b => b.GetAvailableServerParks()).Returns(new List<string>()); 

            //act
            _sut.ProcessFiles(_availableFiles);

            //assert
            _fileServiceMock.Verify(v => v.GetDatabaseFilesAvailable(_availableFiles), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetAvailableServerParks(), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _fileServiceMock.VerifyNoOtherCalls();
            _importFileServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_DataBaseFiles_Available_But_Survey_Does_Not_Exist_When_I_Call_ProcessFiles_Then_Nothing_Is_Processed()
        {
            //arrange
            var surveyName = "NotFound";
            _fileServiceMock.Setup(f => f.GetSurveyNameFromFile(_databaseFileName)).Returns(surveyName);

            //act
            _sut.ProcessFiles(_availableFiles);

            //assert
            _fileServiceMock.Verify(v => v.GetDatabaseFilesAvailable(_availableFiles), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.GetAvailableServerParks(), Times.Once);
            _fileServiceMock.Verify(v => v.GetSurveyNameFromFile(_databaseFileName), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.SurveyExists(_serverParkName, surveyName), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _fileServiceMock.VerifyNoOtherCalls();
            _importFileServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_DataBaseFiles_Available_When_I_Call_ProcessFiles_Then_The_File_Is_Processed()
        {
            //act
            _sut.ProcessFiles(_availableFiles);

            //assert
            _importFileServiceMock.Verify(v => v.ImportSurveyRecordsFromFile(_databaseFileName, _serverParkName, _surveyName), Times.Once);
        }
    }
}
