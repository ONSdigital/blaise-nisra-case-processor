using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;


namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class ImportNisraDataFileService : IImportNisraDataFileService
    {
        private readonly IBlaiseCaseApi _blaiseApi;
        private readonly IImportNisraCaseService _onlineCaseService;
        private readonly ILoggingService _loggingService;

        public ImportNisraDataFileService(
            IBlaiseCaseApi blaiseApi, 
            IImportNisraCaseService onlineCaseService, 
            ILoggingService loggingService)
        {
            _blaiseApi = blaiseApi;
            _onlineCaseService = onlineCaseService;
            _loggingService = loggingService;
        }


        public void ImportNisraDatabaseFile(string serverParkName, string instrumentName, string databaseFilePath)
        {
            var caseRecords = _blaiseApi.GetCases(databaseFilePath);

            while (!caseRecords.EndOfSet)
            {
                var newRecord = caseRecords.ActiveRecord;
                var primaryKey = _blaiseApi.GetPrimaryKeyValue(newRecord);

                if (_blaiseApi.CaseExists(primaryKey, instrumentName, serverParkName))
                {
                    _loggingService.LogInfo($"Case with serial number '{primaryKey}' exists in Blaise");

                    var existingCase = _blaiseApi.GetCase(primaryKey, instrumentName, serverParkName);
                    _onlineCaseService.UpdateExistingCaseWithOnlineData(newRecord, existingCase, 
                        serverParkName, instrumentName,  primaryKey);
                }
                else
                {
                    _loggingService.LogInfo($"Case with serial number '{primaryKey}' does not exist in Blaise");

                    _onlineCaseService.CreateOnlineCase(newRecord, instrumentName, 
                        serverParkName, primaryKey);
                }

                caseRecords.MoveNext();
            }
        }
    }
}
