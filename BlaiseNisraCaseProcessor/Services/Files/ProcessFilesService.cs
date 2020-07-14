using System.Collections.Generic;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Interfaces.Services.Blaise;
using BlaiseNisraCaseProcessor.Interfaces.Services.Files;
using log4net;

namespace BlaiseNisraCaseProcessor.Services.Files
{
    public class ProcessFilesService : IProcessFilesService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IFileService _fileService;
        private readonly IImportFileService _importFileService;

        public ProcessFilesService(
            ILog logger,
            IBlaiseApiService blaiseApiService,
            IFileService fileService,
            IImportFileService importFileService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _fileService = fileService;
            _importFileService = importFileService;
        }

        public void ProcessFiles(IEnumerable<string> filesToProcess)
        {
            var databaseFiles = _fileService.GetDatabaseFilesAvailable(filesToProcess);

            if (!databaseFiles.Any())
            {
                _logger.Info("No database files found in the files to process");
                return;
            }

            foreach (var serverPark in _blaiseApiService.GetAvailableServerParks())
                foreach (var databaseFile in databaseFiles)
                {
                    var surveyName = _fileService.GetSurveyNameFromFile(databaseFile);

                    if (_blaiseApiService.SurveyExists(serverPark, surveyName))
                    {
                        _logger.Info($"Survey '{surveyName}' exists on server park '{serverPark}'");
                        _importFileService.ImportSurveyRecordsFromFile(databaseFile, serverPark, surveyName);
                        continue;
                    }

                    _logger.Info($"Survey '{surveyName}' does not exist on server park '{serverPark}'");
                }
        }
    }
}
