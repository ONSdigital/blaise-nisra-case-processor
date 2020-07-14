using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class ImportFileService : IImportFileService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IUpdateDataRecordByHoutService _updateByHoutService;
        private readonly IUpdateRecordByWebFormStatusService _updateByWebFormStatus;

        public ImportFileService(
            ILog logger,
            IBlaiseApiService blaiseApiService, 
            IUpdateDataRecordByHoutService updateByHoutService, 
            IUpdateRecordByWebFormStatusService updateByWebFormStatus)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _updateByHoutService = updateByHoutService;
            _updateByWebFormStatus = updateByWebFormStatus;
        }

        public void ImportSurveyRecordsFromFile(string databaseFile, string serverPark, string surveyName)
        {
            var cases = _blaiseApiService.GetCasesFromFile(databaseFile);

            while (!cases.EndOfSet)
            {
                var newDataRecord = cases.ActiveRecord;
                var serialNumber = _blaiseApiService.GetSerialNumber(newDataRecord);

                if (!_blaiseApiService.CaseExists(serialNumber, serverPark, surveyName))
                {
                    _blaiseApiService.AddDataRecord(newDataRecord, serverPark, surveyName);
                    _logger.Info($"Added new case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                    cases.MoveNext();
                    continue;
                }

                var existingDataRecord = _blaiseApiService.GetDataRecord(serialNumber, serverPark, surveyName);

                if (_blaiseApiService.WebFormStatusFieldExists(newDataRecord) && _blaiseApiService.WebFormStatusFieldExists(existingDataRecord))
                {
                    _updateByWebFormStatus.UpdateDataRecordViaWebFormStatus(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                    _logger.Info($"Updated case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}'");

                    cases.MoveNext();
                    continue;
                }

                _updateByHoutService.UpdateDataRecordByHoutValues(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                _logger.Info($"Updated case with serial number {serialNumber} to survey '{surveyName}' on server park '{serverPark}' via HOut values");

                cases.MoveNext();
            }
        }
    }
}
