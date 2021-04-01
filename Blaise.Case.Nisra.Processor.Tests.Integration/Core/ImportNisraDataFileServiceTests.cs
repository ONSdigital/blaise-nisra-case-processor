using System.Diagnostics;
using System.IO;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Tests.Integration.Helpers;
using Blaise.Case.Nisra.Processor.WindowsService.Ioc;
using NUnit.Framework;

namespace Blaise.Case.Nisra.Processor.Tests.Integration.Core
{
    public class ImportNisraDataFileServiceTests
    {
        private readonly string _serverParkName;
        private readonly string _instrumentName;
        private readonly string _instrumentPackage;
        private readonly string _tempExtractPath;
        private readonly string _databaseFileName;

        private readonly Stopwatch _timer;


        private IImportNisraDataFileService _sut;

        public ImportNisraDataFileServiceTests()
        {
            _serverParkName = "LocalDevelopment";
            _instrumentName = "OPN2102R";
            _instrumentPackage = @"D:\Temp\Nisra\OPN2102R.zip";
            _databaseFileName = @"D:\Temp\Nisra\OPN2102R.zip";
            _tempExtractPath = @"D:\Temp\Nisra\temp";
            _databaseFileName = $@"{_tempExtractPath}\\{_instrumentName}\\{_instrumentName}.bdix";

            _timer = new Stopwatch();
        }
        
        [SetUp]
        public void SetUpTests()
        {
            var unityProvider = new UnityProvider();
            _sut = unityProvider.Resolve<IImportNisraDataFileService>();
        }

        [Test]
        public void Given_I_Call_ImportOnlineDatabaseFile_Then_Records_Are_Processed_Efficiently()
        {
            //arrange
            //CaseHelper.GetInstance().DeleteCasesInBlaise(_instrumentName, _serverParkName);
            //Directory.Delete(_tempExtractPath, true);
            CaseHelper.GetInstance().CreateCasesInBlaise(3000, _instrumentName, _serverParkName, "0");
            NisraFileHelper.GetInstance().CreateCasesInInstrumentFile(3000, _instrumentName, _instrumentPackage, _tempExtractPath, "110");

            //act
            _timer.Start();
            _sut.ImportNisraDatabaseFile(_serverParkName, _instrumentName, _databaseFileName);
            _timer.Stop();

            //assert
            var elapsedMinutes = _timer.ElapsedMilliseconds / 1000 / 60;
            Assert.Pass();
        }

        [TearDown]
        public void TearDownTests()
        {
            //CaseHelper.GetInstance().DeleteCasesInBlaise(_instrumentName, _serverParkName);
            //Directory.Delete(_tempExtractPath, true);
        }
    }
}
