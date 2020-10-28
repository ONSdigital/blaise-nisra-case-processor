using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Blaise.Nisra.Case.Processor.Interfaces.Services;
using Blaise.Nisra.Case.Processor.Services;
using log4net;
using Moq;
using NUnit.Framework;

namespace Blaise.Nisra.Case.Processor.Tests.Unit.Services
{
    public class ProcessFilesServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IBlaiseApiService> _blaiseApiServiceMock;
        private Mock<IImportCasesService> _importFileServiceMock;
        private MockFileSystem _mockFileSystem;

        private readonly List<string> _availableFiles;
        private readonly List<string> _serverParks;
        private readonly string _databaseFileName;
        private readonly string _serverParkName;
        private readonly string _surveyName;

        private ProcessFilesService _sut;

        public ProcessFilesServiceTests()
        {
            _serverParkName = "Park1";
            _surveyName = "OPN123";
            _databaseFileName = "OPN123.bdix";

            _availableFiles = new List<string> { "OPN123.bdbx", "OPN123.bdix" };
            _serverParks = new List<string> { _serverParkName };
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();

            _blaiseApiServiceMock = new Mock<IBlaiseApiService>();
            _blaiseApiServiceMock.Setup(b => b.GetAvailableServerParks()).Returns(_serverParks);
            _blaiseApiServiceMock.Setup(b => b.SurveyExists(_serverParkName, _surveyName)).Returns(true);

            _mockFileSystem = new MockFileSystem();
            _mockFileSystem.AddFile(_databaseFileName, new MockFileData(""));

            _importFileServiceMock = new Mock<IImportCasesService>();

            _sut = new ProcessFilesService(
                _loggingMock.Object,
                _blaiseApiServiceMock.Object,
                _importFileServiceMock.Object,
                _mockFileSystem);
        }

        [Test]
        public void Given_No_Database_Files_Are_In_The_Files_List_When_I_Call_ProcessFiles_Then_Nothing_Is_Processed()
        {
            //arrange
            var fileListWithNoDatabaseFiles = new List<string> { "OPN123.bdbx", "OPN123.bix" };

            //act
            _sut.ProcessFiles(fileListWithNoDatabaseFiles);

            //assert
            _blaiseApiServiceMock.VerifyNoOtherCalls();
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
            _blaiseApiServiceMock.Verify(v => v.GetAvailableServerParks(), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _importFileServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_DataBaseFiles_Available_But_Survey_Does_Not_Exist_When_I_Call_ProcessFiles_Then_Nothing_Is_Processed()
        {
            //arrange
            var surveyName = "NotFound";
            var fileList = new List<string> { $"{surveyName}.bdbx", $"{surveyName}.bdix" };

            //act
            _sut.ProcessFiles(fileList);

            //assert
            _blaiseApiServiceMock.Verify(v => v.GetAvailableServerParks(), Times.Once);
            _blaiseApiServiceMock.Verify(v => v.SurveyExists(_serverParkName, surveyName), Times.Once);

            _blaiseApiServiceMock.VerifyNoOtherCalls();
            _importFileServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_DataBaseFiles_Available_When_I_Call_ProcessFiles_Then_The_File_Is_Processed()
        {
            //act
            _sut.ProcessFiles(_availableFiles);

            //assert
            _importFileServiceMock.Verify(v => v.ImportCasesFromFile(_databaseFileName, _serverParkName, _surveyName), Times.Once);
        }
    }
}
