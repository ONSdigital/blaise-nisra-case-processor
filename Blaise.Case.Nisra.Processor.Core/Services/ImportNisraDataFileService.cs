using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;


namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class ImportNisraDataFileService : IImportNisraDataFileService
    {
        private readonly IBlaiseCaseApi _blaiseApi;
        private readonly IImportNisraCaseService _nisraCaseService;
        private readonly ILoggingService _loggingService;

        public ImportNisraDataFileService(
            IBlaiseCaseApi blaiseApi,
            IImportNisraCaseService nisraCaseService, 
            ILoggingService loggingService)
        {
            _blaiseApi = blaiseApi;
            _nisraCaseService = nisraCaseService;
            _loggingService = loggingService;
        }


        public void ImportNisraDatabaseFile(string serverParkName, string instrumentName, string databaseFilePath)
        {
            var caseRecords = _blaiseApi.GetCases(databaseFilePath);

            while (!caseRecords.EndOfSet)
            {
                var nisraRecord = caseRecords.ActiveRecord;
                var primaryKey = _blaiseApi.GetPrimaryKeyValue(nisraRecord);
                var nisraOutcomeCode = _blaiseApi.GetOutcomeCode(nisraRecord);

                if (nisraOutcomeCode == 0)
                {
                    _loggingService.LogInfo($"Not processed: NISRA case '{primaryKey}' (NISRA HOut = 0) for instrument '{instrumentName}'");
                }
                else
                {
                    _nisraCaseService.ImportNisraCase(nisraRecord, instrumentName, serverParkName);
                }

                caseRecords.MoveNext();
            }
        }
    }
}
