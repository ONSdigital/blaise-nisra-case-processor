using System;
using System.Collections.Generic;
using BlaiseNisraCaseProcessor.Interfaces.Services.Files;
using BlaiseNisraCaseProcessor.Services;
using log4net;
using Moq;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class ProcessNisraFilesServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<ICloudBucketFileService> _cloudBucketFileServiceMock;
        private Mock<IProcessFilesService> _processFilesServiceMock;

        private readonly List<string> _availableFiles;

        private ProcessNisraFilesService _sut;

        public ProcessNisraFilesServiceTests()
        {
            _availableFiles = new List<string> { "File1.bdbx", "File2.bdbx" };
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();

            _cloudBucketFileServiceMock = new Mock<ICloudBucketFileService>();

            _processFilesServiceMock = new Mock<IProcessFilesService>();

            _sut = new ProcessNisraFilesService(
                _loggingMock.Object,
                _cloudBucketFileServiceMock.Object,
                _processFilesServiceMock.Object);
        }

        [Test]
        public void Given_Files_Are_Available_When_I_Call_DownloadAndProcessAvailableFiles_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            _cloudBucketFileServiceMock.Setup(c => c.GetFilesFromBucket()).Returns(_availableFiles);

            //act
            _sut.DownloadAndProcessAvailableFiles();

            //assert
            _cloudBucketFileServiceMock.Verify(v => v.GetFilesFromBucket(), Times.Once);
            _processFilesServiceMock.Verify(v => v.ProcessFiles(_availableFiles), Times.Once);
        }

        [Test]
        public void Given_No_Files_Are_Available_When_I_Call_DownloadAndProcessAvailableFiles_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            _cloudBucketFileServiceMock.Setup(c => c.GetFilesFromBucket()).Returns(new List<string>());

            //act
            _sut.DownloadAndProcessAvailableFiles();

            //assert
            _cloudBucketFileServiceMock.Verify(v => v.GetFilesFromBucket(), Times.Once);
            _processFilesServiceMock.Verify(v => v.ProcessFiles(_availableFiles), Times.Never);
        }

        [Test]
        public void Given_An_Error_Occurs_When_Getting_Files_From_The_Bucket_When_I_Call_DownloadAndProcessAvailableFiles_Then_Exception_Is_Handled_Correctly()
        {
            //arrange
            _cloudBucketFileServiceMock.Setup(c => c.GetFilesFromBucket()).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.DownloadAndProcessAvailableFiles());
        }

        [Test]
        public void Given_An_Error_Occurs_When_Getting_Files_From_The_Bucket_When_I_Call_DownloadAndProcessAvailableFiles_Then_ProcessFiles_Is_Not_Called()
        {
            //arrange
            _cloudBucketFileServiceMock.Setup(c => c.GetFilesFromBucket()).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.DownloadAndProcessAvailableFiles());
            _processFilesServiceMock.Verify(v => v.ProcessFiles(It.IsAny<List<string>>()), Times.Never);
        }

        [Test]
        public void Given_An_Error_Occurs_When_Processing_Files_When_I_Call_DownloadAndProcessAvailableFiles_Then_Exception_Is_Handled_Correctly()
        {
            //arrange
            _cloudBucketFileServiceMock.Setup(c => c.GetFilesFromBucket()).Returns(_availableFiles);
            _processFilesServiceMock.Setup(c => c.ProcessFiles(_availableFiles)).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.DownloadAndProcessAvailableFiles());
        }
    }
}
