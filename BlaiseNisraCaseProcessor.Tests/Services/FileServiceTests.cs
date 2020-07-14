using System.Collections.Generic;
using System.IO.Abstractions;
using BlaiseNisraCaseProcessor.Services;
using Moq;
using NUnit.Framework;

namespace BlaiseNisraCaseProcessor.Tests.Services
{
    public class FileServiceTests
    {
        private Mock<IFileSystem> _fileSystemMock;

        private FileService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();

            _sut = new FileService(_fileSystemMock.Object);
        }

        [Test]
        public void Given_A_List_Of_Files_That_Contains_DatabaseFiles_When_I_Call_GetDatabaseFilesAvailable_The_Correct_Files_Are_Returned()
        {
            //arrange
            var files = new List<string>
            {
                "File1.bdix",
                "File1.bdbx",
                "File2.bdix",
                "File2.bdbx"
            };
            
            //act
            var result = _sut.GetDatabaseFilesAvailable(files);

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.AreEqual(2, result.Count);
            Assert.True(result.Contains("File1.bdix"));
            Assert.True(result.Contains("File2.bdix"));
        }
    }
}
