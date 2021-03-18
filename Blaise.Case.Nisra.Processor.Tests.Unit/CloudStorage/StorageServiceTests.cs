using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.CloudStorage.Services;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Moq;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Unit.CloudStorage
{
    public class StorageServiceTests
    {
        private StorageService _sut;

        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<ICloudStorageClientProvider> _storageProviderMock;
        private Mock<IFileSystem> _fileSystemMock;
        private Mock<ILoggingService> _loggingMock;

        [SetUp]
        public void SetUpTests()
        {
            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _storageProviderMock = new Mock<ICloudStorageClientProvider>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggingMock = new Mock<ILoggingService>();

            _sut = new StorageService(
                _configurationProviderMock.Object,
                _storageProviderMock.Object,
                _fileSystemMock.Object,
                _loggingMock.Object);
        }

        
        [Test]
        public void Given_I_Call_DownloadDatabaseFilesFromNisraBucketAsync_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            const string instrumentName = "OPN2101A";
            const string bucketName = "NISRA";
            const string bucketFilePath = "OPN1234";
            const string tempFilePath = @"d:\temp\GUID";
            var files = new List<string>
            {
                "OPN.bdix",
                "OPN.blix",
                "OPN.bmix"
            };

            _storageProviderMock.Setup(s => s.GetListOfFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(files);

            foreach (var file in files)
            {
                _fileSystemMock.Setup(f => f.Path.GetFileName(file)).Returns(file);
                _fileSystemMock.Setup(s => s.Path.Combine(tempFilePath, file)).Returns($@"{tempFilePath}\\{file}");
            }

            _configurationProviderMock.Setup(c => c.BucketName).Returns(bucketName);
            _configurationProviderMock.Setup(c => c.LocalTempFolder).Returns(tempFilePath);

            _fileSystemMock.Setup(f => f.Directory.Exists(tempFilePath)).Returns(true);

            //act
            _sut.GetInstrumentFileFromBucket(instrumentName, bucketFilePath);

            //assert
            _storageProviderMock.Verify(v => v.GetListOfFiles(bucketName,
                bucketFilePath), Times.Once);

            foreach (var file in files)
            {
                _storageProviderMock.Verify(v => v.Download(bucketName, file, $@"{tempFilePath}\\{file}"));
            }
        }

        [Test]
        public void Given_I_Call_DownloadDatabaseFilesFromNisraBucketAsync_Then_The_Correct_FileName_Is_Returned()
        {
            //arrange
            const string instrumentName = "OPN2101A";
            const string bucketName = "NISRA";
            const string bucketFilePath = "OPN1234";
            const string tempFilePath = @"d:\temp\GUID";
            var files = new List<string>
            {
                "OPN.bdix",
                "OPN.blix",
                "OPN.bmix"
            };
            var databaseFileName = $"{tempFilePath}\\{instrumentName}.bdix";

            _storageProviderMock.Setup(s => s.GetListOfFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(files);

            foreach (var file in files)
            {
                _fileSystemMock.Setup(f => f.Path.GetFileName(file)).Returns(file);
                _fileSystemMock.Setup(s => s.Path.Combine(tempFilePath, file)).Returns($@"{tempFilePath}\\{file}");
            }

            _configurationProviderMock.Setup(c => c.BucketName).Returns(bucketName);
            _configurationProviderMock.Setup(c => c.LocalTempFolder).Returns(tempFilePath);

            _fileSystemMock.Setup(f => f.Directory.Exists(tempFilePath)).Returns(true);
            _fileSystemMock.Setup(f => f.Path.Combine(tempFilePath, $"{instrumentName}.bdix"))
                .Returns(databaseFileName);

            //act
            var result =_sut.GetInstrumentFileFromBucket(instrumentName, bucketFilePath);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(databaseFileName, result);
        }

        [Test]
        public void Given_No_Files_Are_In_The_BucketPath_When_I_Call_DownloadDatabaseFilesFromNisraBucketAsync_Then_An_Exception_Is_Thrown()
        {
            //arrange
            const string instrumentName = "OPN2101A";
            const string bucketName = "NISRA";
            const string bucketFilePath = "OPN1234";

            _storageProviderMock.Setup(s => s.GetListOfFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<string>());

            _configurationProviderMock.Setup(c => c.BucketName).Returns(bucketName);

            //act && assert
            var exception = Assert.Throws<Exception>(() => _sut.GetInstrumentFileFromBucket(instrumentName, bucketFilePath));
            Assert.AreEqual($"No files were found for bucket path '{bucketFilePath}' in bucket '{bucketName}'", exception.Message);
        }

        [Test]
        public void Given_I_Call_DeleteDownloadedFiles_Then_The_Correct_Folder_Is_Deleted()
        {
            //arrange
            const string tempFilePath = @"d:\temp\GUID";
            _configurationProviderMock.Setup(c => c.LocalTempFolder).Returns(tempFilePath);
            _fileSystemMock.Setup(f => f.Directory.Delete(It.IsAny<string>(), It.IsAny<bool>()));

            //act
            _sut.DeleteDownloadedFiles();

            //assert
            _fileSystemMock.Verify(f => f.Directory.Delete(tempFilePath, true), Times.Once);
        }

        [Test]
        public void Given_I_Call_DownloadFromBucket_Then_The_Correct_Services_Are_Called()
        {
            //arrange
            const string bucketName = "OPN";
            const string bucketFilePath = "OPN1234/OPN1234.zip";
            const string fileName = "OPN1234.zip";
            const string filePath = @"d:\temp";
            var destinationFilePath = $@"{filePath}\{fileName}";

            _fileSystemMock.Setup(f => f.Directory.Exists(filePath)).Returns(true);
            _fileSystemMock.Setup(f => f.Path.GetFileName(bucketFilePath)).Returns(fileName);
            _fileSystemMock.Setup(s => s.Path.Combine(filePath, fileName))
                .Returns(destinationFilePath);
            _fileSystemMock.Setup(s => s.File.Delete(It.IsAny<string>()));

            //act
            _sut.DownloadFileFromBucket(bucketName, bucketFilePath, filePath);

            //assert
            _storageProviderMock.Verify(v => v.Download(bucketName,
                bucketFilePath, destinationFilePath));
        }

        [Test]
        public void Given_I_Call_DownloadFromBucket_Then_The_Correct_FilePath_Is_Returned()
        {
            //arrange
            const string bucketName = "OPN";
            const string bucketFilePath = "OPN1234/OPN1234.zip";
            const string fileName = "OPN1234.zip";
            const string filePath = @"d:\temp";
            var destinationFilePath = $@"{filePath}\{fileName}";

            _fileSystemMock.Setup(f => f.Directory.Exists(filePath)).Returns(true);
            _fileSystemMock.Setup(f => f.Path.GetFileName(bucketFilePath)).Returns(fileName);
            _fileSystemMock.Setup(s => s.Path.Combine(filePath, fileName))
                .Returns(destinationFilePath);
            _fileSystemMock.Setup(s => s.File.Delete(It.IsAny<string>()));

            //act
            var result = _sut.DownloadFileFromBucket(bucketName, bucketFilePath, filePath);

            //arrange
            Assert.AreEqual(destinationFilePath, result);
        }

        [Test]
        public void Given_Temp_Path_Is_Not_There_When_I_Call_DownloadFromBucket_Then_The_Temp_Path_Is_Created()
        {
            //arrange
            const string bucketName = "OPN";
            const string fileName = "OPN1234.zip";
            const string filePath = @"d:\temp";
            var destinationFilePath = $@"{filePath}\{fileName}";

            _fileSystemMock.Setup(f => f.Directory.Exists(filePath)).Returns(false);
            _fileSystemMock.Setup(s => s.Path.Combine(filePath, fileName))
                .Returns(destinationFilePath);

            _storageProviderMock.Setup(s => s.Download(bucketName, fileName, filePath));
            _fileSystemMock.Setup(s => s.File.Delete(It.IsAny<string>()));

            //act
            _sut.DownloadFileFromBucket(bucketName, fileName, filePath);

            //arrange
            _fileSystemMock.Verify(v => v.Directory.CreateDirectory(filePath), Times.Once);
        }

        [Test]
        public void Given_I_Call_GetDatabaseFile_Then_The_Correct_Name_Is_Returned()
        {
            //arrange
            const string instrumentName = "OPN2101A";
            const string filePath = @"d:\test";
            var expectedName = $@"{filePath}\{instrumentName}.bdix";

            var sut = new StorageService(
                _configurationProviderMock.Object,
                _storageProviderMock.Object,
               new MockFileSystem(), 
                _loggingMock.Object);

            //act
            var result = sut.GetDatabaseFile(instrumentName, filePath);

            //assert
            Assert.AreEqual(expectedName, result);
        }
    }
}