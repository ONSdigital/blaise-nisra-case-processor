using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Enums;
using Blaise.Nuget.Api.Contracts.Interfaces;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class ImportNisraCaseService : IImportNisraCaseService
    {
        private readonly IBlaiseCaseApi _blaiseApi;
        private readonly IBlaiseCaseService _blaiseCaseService;
        private readonly ILoggingService _loggingService;

        public ImportNisraCaseService(
            IBlaiseCaseApi blaiseApi,
            IBlaiseCaseService blaiseCaseService,
            ILoggingService loggingService)
        {
            _blaiseApi = blaiseApi;
            _blaiseCaseService = blaiseCaseService;
            _loggingService = loggingService;
        }

        public void ImportNisraCase(IDataRecord nisraDataRecord, string instrumentName, string serverParkName)
        {
            var primaryKey = _blaiseApi.GetPrimaryKeyValue(nisraDataRecord);

            if (_blaiseApi.CaseExists(primaryKey, instrumentName, serverParkName))
            {
                var existingDataRecord = _blaiseApi.GetCase(primaryKey, instrumentName, serverParkName);
                UpdateExistingCase(nisraDataRecord, existingDataRecord, instrumentName, serverParkName, primaryKey);
                return;
            }

            //_blaiseCaseService.CreateCase(nisraDataRecord, instrumentName, serverParkName, primaryKey);
        }

        public void UpdateExistingCase(IDataRecord nisraDataRecord, IDataRecord existingDataRecord,
             string instrumentName, string serverParkName, string primaryKey)
        {
            var nisraOutcome = _blaiseApi.GetOutcomeCode(nisraDataRecord);
            var existingOutcome = _blaiseApi.GetOutcomeCode(existingDataRecord);

            if (nisraOutcome == 0)
            {
                return;
            }

            if (existingOutcome == 561 || existingOutcome == 562)
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' (Existing HOut = '{existingOutcome}' for instrument '{instrumentName}'");

                return;
            }

            if (NisraRecordHasAlreadyBeenProcessed(nisraDataRecord, nisraOutcome, existingDataRecord,
                existingOutcome, primaryKey, instrumentName))
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' as is has already been updated on a previous run for instrument '{instrumentName}'");

                return;
            }

            //if (_blaiseApi.CaseInUseInCati(existingDataRecord))
            //{
            //    _loggingService.LogInfo(
            //        $"Not processed: NISRA case '{primaryKey}' as the case may be open in Cati for instrument '{instrumentName}'");

            //    return;
            //}

            if (existingOutcome > 0 && existingOutcome < nisraOutcome)
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' (Existing HOut = '{existingOutcome}' < '{nisraOutcome}')  for instrument '{instrumentName}'");

                return;
            }

            _loggingService.LogInfo(
                $"processed: NISRA case '{primaryKey}' (NISRA HOut = '{nisraOutcome}' <= '{existingOutcome}') or (Existing HOut = 0)' for instrument '{instrumentName}'");

            //_blaiseCaseService.UpdateCase(nisraDataRecord, existingDataRecord, instrumentName, serverParkName,  primaryKey);
        }

        internal bool NisraRecordHasAlreadyBeenProcessed(IDataRecord nisraDataRecord, int nisraOutcome,
            IDataRecord existingDataRecord, int existingOutcome, string primaryKey, string instrumentName)
        {
            var nisraTimeStamp = _blaiseApi.GetFieldValue(nisraDataRecord, FieldNameType.LastUpdated);
            var existingTimeStamp = _blaiseApi.GetFieldValue(existingDataRecord, FieldNameType.LastUpdated);

            var recordHasAlreadyBeenProcessed = nisraOutcome == existingOutcome && nisraTimeStamp == existingTimeStamp;

            _loggingService.LogInfo($"Check NISRA Update needed for case '{primaryKey}': '{!recordHasAlreadyBeenProcessed}' - " +
                                    $"(NISRA HOut = '{nisraOutcome}' timestamp = '{nisraTimeStamp}') " +
                                    $"(Existing HOut = '{existingOutcome}' timestamp = '{existingTimeStamp}')" +
                                    $" for instrument '{instrumentName}'");

            return recordHasAlreadyBeenProcessed;
        }
    }
}
