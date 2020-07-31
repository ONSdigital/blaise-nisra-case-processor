using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class ImportCasesService : IImportCasesService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IUpdateCaseByHoutService _updateByHoutService;
        private readonly IUpdateCaseServiceService _updateByWebFormStatus;

        public ImportCasesService(
            ILog logger,
            IBlaiseApiService blaiseApiService, 
            IUpdateCaseByHoutService updateByHoutService, 
            IUpdateCaseServiceService updateByWebFormStatus)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _updateByHoutService = updateByHoutService;
            _updateByWebFormStatus = updateByWebFormStatus;
        }

        public void ImportCasesFromFile(string databaseFile, string serverPark, string surveyName)
        {
            var cases = _blaiseApiService.GetCasesFromFile(databaseFile);

            while (!cases.EndOfSet)
            {
                var newDataRecord = cases.ActiveRecord;
                var serialNumber = _blaiseApiService.GetSerialNumber(newDataRecord);

                if (!_blaiseApiService.CaseExists(serialNumber, serverPark, surveyName))
                {
                    _blaiseApiService.AddDataRecord(newDataRecord, serialNumber, serverPark, surveyName);
                    _logger.Info($"Added new case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                    cases.MoveNext();
                    continue;
                }

                var existingDataRecord = _blaiseApiService.GetDataRecord(serialNumber, serverPark, surveyName);

                if (_blaiseApiService.WebFormStatusFieldExists(newDataRecord) && _blaiseApiService.WebFormStatusFieldExists(existingDataRecord))
                {
                    _updateByWebFormStatus.UpdateCase(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                    _logger.Info($"Updated case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                    cases.MoveNext();
                    continue;
                }

                _updateByHoutService.UpdateCaseByHoutValues(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                _logger.Info($"Updated case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}' via HOut values");

                cases.MoveNext();
            }
        }
    }
}
