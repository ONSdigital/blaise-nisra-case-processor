using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Data.Interfaces;
using log4net;

namespace Blaise.Case.Nisra.Processor.Core
{
    public class ProcessNisraFilesService : IProcessNisraFilesService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IProcessNisraCasesService _importFileService;
        private readonly IFileSystem _fileSystem;

        private const string DatabaseFileNameExt = ".bdix";

        public ProcessNisraFilesService(
            ILog logger,
            IBlaiseApiService blaiseApiService,
            IProcessNisraCasesService importFileService, 
            IFileSystem fileSystem)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _importFileService = importFileService;
            _fileSystem = fileSystem;
        }

        public void ProcessFiles(IList<string> filesToProcess)
        {
            var databaseFiles = GetDatabaseFilesAvailable(filesToProcess);

            if (!databaseFiles.Any())
            {
                _logger.Info("No database files found in the files to process");
                return;
            }

            foreach (var serverPark in _blaiseApiService.GetAvailableServerParks())
                foreach (var databaseFile in databaseFiles)
                {
                    var surveyName = GetSurveyNameFromFile(databaseFile);

                    if (_blaiseApiService.SurveyExists(serverPark, surveyName))
                    {
                        _logger.Info($"Survey '{surveyName}' exists on server park '{serverPark}'");
                        _importFileService.ProcessNisraCases(databaseFile, serverPark, surveyName);
                        continue;
                    }

                    _logger.Info($"Survey '{surveyName}' does not exist on server park '{serverPark}'");
                }

            DeleteTemporaryFiles(filesToProcess);
        }


        private static List<string> GetDatabaseFilesAvailable(IEnumerable<string> files)
        {
            return files.Where(f => f.ToLower().Contains(DatabaseFileNameExt)).ToList();
        }

        private string GetSurveyNameFromFile(string databaseFile)
        {
            return _fileSystem.Path.GetFileNameWithoutExtension(databaseFile);
        }

        private void DeleteTemporaryFiles(IEnumerable<string> filesToProcess)
        {
            foreach (var file in filesToProcess)
            {
                _fileSystem.File.Delete(file);
            }
        }
    }
}
