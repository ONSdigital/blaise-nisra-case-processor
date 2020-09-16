using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class CloudStorageServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<IStorageClientProvider> _storageClientProviderMock;
        private MockFileSystem _mockFileSystem;

        private readonly string _localPath;
        private readonly string _file1;
        private readonly string _file2;

        private CloudStorageService _sut;

        public CloudStorageServiceTests()
        {
            _localPath = @"c:\temp";
            _file1 = "OPN123.bdx";
            _file2 = "OPN123.bdix";
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();

            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(c => c.LocalProcessFolder)
                .Returns(_localPath);

            _storageClientProviderMock = new Mock<IStorageClientProvider>();

            _mockFileSystem = new MockFileSystem();

            _sut = new CloudStorageService(
                _loggingMock.Object,
                _configurationProviderMock.Object,
                _storageClientProviderMock.Object,
                _mockFileSystem);
        }

        [Test]
        public void Given_There_Are_No_Files_In_The_Bucket_When_I_Call_GetFilesFromBucket_Then_An_Empty_List_Is_Returned()
        {
            //arrange
            var filesInBucket = new List<string>();

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            var result = _sut.GetAvailableFilesFromBucket();

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void Given_There_Are_No_Files_In_The_Bucket_When_I_Call_GetFilesFromBucket_Then_No_Files_Are_Downloaded()
        {
            //arrange
            var filesInBucket = new List<string>();

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            _sut.GetAvailableFilesFromBucket();

            //assert
            _storageClientProviderMock.Verify(v => v.Download(
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Given_There_Are_No_Files_In_The_Bucket_When_I_Call_GetFilesFromBucket_Then_The_Storage_Client_Is_Disposed()
        {
            //arrange
            var filesInBucket = new List<string>();

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            _sut.GetAvailableFilesFromBucket();

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);
        }

        [Test]
        public void Given_There_Are_Files_In_The_Bucket_When_I_Call_DownloadFilesFromBucket_Then_The_Files_Are_Downloaded()
        {
            //arrange
            var filesInBucket = new List<string>
            {
                _file1,
                _file2
            };

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            _sut.DownloadFilesFromBucket(filesInBucket);

            //assert
            _storageClientProviderMock.Verify(v => v.Download(
                _file1, $"{_localPath}\\{_file1}"), Times.Once);

            _storageClientProviderMock.Verify(v => v.Download(
                _file2, $"{_localPath}\\{_file2}"), Times.Once);
        }

        [Test]
        public void Given_There_Are_Files_In_The_Bucket_When_I_Call_GetFilesFromBucket_Then_A_List_Of_Files_Gets_Returned()
        {
            //arrange
            var filesInBucket = new List<string>
            {
                _file1,
                _file2
            };

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            var result = _sut.GetAvailableFilesFromBucket().ToList();

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains($"{_file1}"));
            Assert.IsTrue(result.Contains($"{_file2}"));
        }

        [Test]
        public void Given_There_Are_Files_In_The_Bucket_When_I_Call_GetFilesFromBucket_Then_The_Storage_Client_Is_Disposed()
        {
            //arrange
            var filesInBucket = new List<string>
            {
                _file1,
                _file2
            };

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            _sut.GetAvailableFilesFromBucket();

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);
        }

        [Test]
        public void Given_There_Are_Files_In_The_Bucket_When_I_Call_DownloadFilesFromBucket_Then_A_List_Of_Files_Gets_Returned()
        {
            //arrange
            var filesInBucket = new List<string>
            {
                _file1,
                _file2
            };

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            var result = _sut.DownloadFilesFromBucket(filesInBucket).ToList();

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains($"{_localPath}\\{_file1}"));
            Assert.IsTrue(result.Contains($"{_localPath}\\{_file2}"));
        }

        [Test]
        public void Given_There_Are_Files_In_The_Bucket_When_I_Call_DownloadFilesFromBucket_Then_The_Storage_Client_Is_Disposed()
        {
            //arrange
            var filesInBucket = new List<string>
            {
                _file1,
                _file2
            };

            _storageClientProviderMock.Setup(s => s.GetAvailableFilesFromBucket())
                .Returns(filesInBucket);

            //act
            _sut.DownloadFilesFromBucket(filesInBucket);

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);
        }

        [Test]
        public void Given_There_Are_No_Files_To_Process_When_I_Call_MoveProcessedFilesToProcessedFolder_Then_Nothing_Is_Processed()
        {
            //arrange
            var filesToProcess = new List<string>();


            //act
            _sut.MoveProcessedFilesToProcessedFolder(filesToProcess);

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);

            _storageClientProviderMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_There_Are_No_Files_To_Process_When_I_Call_MoveProcessedFilesToProcessedFolder_Then_The_Storage_Client_Is_Disposed()
        {
            //arrange
            var filesToProcess = new List<string>();


            //act
            _sut.MoveProcessedFilesToProcessedFolder(filesToProcess);

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);
        }

        [Test]
        public void Given_There_Are_Files_To_Process_When_I_Call_MoveProcessedFilesToProcessedFolder_Then_The_Files_Are_Downloaded()
        {
            //arrange
            var filesToProcess = new List<string>
            {
                $"{_localPath}/{_file1}",
                $"{_localPath}/{_file2}"
            };

            //act
            _sut.MoveProcessedFilesToProcessedFolder(filesToProcess);

            //assert
            _storageClientProviderMock.Verify(v => v.MoveFileToProcessedFolder(
                $"{_localPath}/{_file1}"), Times.Once);

            _storageClientProviderMock.Verify(v => v.MoveFileToProcessedFolder(
                $"{_localPath}/{_file2}"), Times.Once);
        }

        [Test]
        public void Given_There_Are_Files_To_Process_When_I_Call_MoveProcessedFilesToProcessedFolder_Then_The_Storage_Client_Is_Disposed()
        {
            //arrange
            var filesToProcess = new List<string>
            {
                $"{_localPath}/{_file1}",
                $"{_localPath}/{_file2}"
            };

            //act
            _sut.MoveProcessedFilesToProcessedFolder(filesToProcess);

            //assert
            _storageClientProviderMock.Verify(v => v.Dispose(), Times.Once);
        }
    }
}
