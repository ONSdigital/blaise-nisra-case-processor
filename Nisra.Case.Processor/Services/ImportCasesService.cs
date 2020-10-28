using log4net;
using Nisra.Case.Processor.Interfaces.Services;

namespace Nisra.Case.Processor.Services
{
    public class ImportCasesService : IImportCasesService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IUpdateCaseService _updateCaseService;

        public ImportCasesService(
            ILog logger,
            IBlaiseApiService blaiseApiService,
            IUpdateCaseService updateCaseService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _updateCaseService = updateCaseService;
        }

        public void ImportCasesFromFile(string databaseFile, string serverPark, string surveyName)
        {
            var cases = _blaiseApiService.GetCasesFromFile(databaseFile);

            while (!cases.EndOfSet)
            {
                var newDataRecord = cases.ActiveRecord;
                var serialNumber = _blaiseApiService.GetSerialNumber(newDataRecord);

                if (_blaiseApiService.CaseExists(serialNumber, serverPark, surveyName))
                {
                    var existingDataRecord = _blaiseApiService.GetDataRecord(serialNumber, serverPark, surveyName);
                    _updateCaseService.UpdateCase(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                    _logger.Info($"Updated case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                    cases.MoveNext();
                    continue;
                }

                _blaiseApiService.AddDataRecord(newDataRecord, serialNumber, serverPark, surveyName);
                _logger.Info($"Added new case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                cases.MoveNext();
            }
        }
    }
}
