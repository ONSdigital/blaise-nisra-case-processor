using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Blaise.Nisra.Case.Processor.Interfaces.Services;
using log4net;

namespace Blaise.Nisra.Case.Processor.Services
{
    public class ProcessFilesService : IProcessFilesService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IImportCasesService _importFileService;
        private readonly IFileSystem _fileSystem;

        private const string DatabaseFileNameExt = ".bdix";

        public ProcessFilesService(
            ILog logger,
            IBlaiseApiService blaiseApiService,
            IImportCasesService importFileService, 
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
                        _importFileService.ImportCasesFromFile(databaseFile, serverPark, surveyName);
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
