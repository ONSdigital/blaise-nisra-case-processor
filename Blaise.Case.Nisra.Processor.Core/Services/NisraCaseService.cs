using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;
using StatNeth.Blaise.API.DataRecord;

[assembly: InternalsVisibleTo("Blaise.Case.Nisra.Processor.Tests.Unit")]
namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class NisraCaseService : INisraCaseService
    {
        private readonly IBlaiseCaseApi _blaiseApi;
        private readonly ICatiDataService _catiDataService;
        private readonly ILoggingService _loggingService;

        public NisraCaseService(
            IBlaiseCaseApi blaiseApi,
            ICatiDataService catiDataService,
            ILoggingService loggingService)
        {
            _blaiseApi = blaiseApi;
            _catiDataService = catiDataService;
            _loggingService = loggingService;
        }

        public void CreateOnlineCase(IDataRecord dataRecord, string instrumentName, string serverParkName,
            string primaryKey)
        {
            var outcomeCode = _blaiseApi.GetOutcomeCode(dataRecord);
            var existingFieldData = _blaiseApi.GetRecordDataFields(dataRecord);

            var newFieldData = _blaiseApi.GetRecordDataFields(dataRecord);
            _catiDataService.RemoveCatiManaBlock(newFieldData);

            _catiDataService.AddCatiManaCallItems(newFieldData, existingFieldData, outcomeCode);

            _blaiseApi.CreateCase(primaryKey, newFieldData, instrumentName, serverParkName);
            _loggingService.LogInfo($"Created new case with SerialNumber '{primaryKey}'");
        }

        public void UpdateExistingCaseWithOnlineData(IDataRecord nisraDataRecord, IDataRecord existingDataRecord,
            string serverParkName, string instrumentName, string primaryKey)
        {
            var nisraOutcome = _blaiseApi.GetOutcomeCode(nisraDataRecord);
            var existingOutcome = _blaiseApi.GetOutcomeCode(existingDataRecord);

            if (nisraOutcome == 0)
            {
                _loggingService.LogInfo($"Not processed: NISRA case '{primaryKey}' (NISRA HOut = 0)");

                return;
            }

            if (existingOutcome == 561 || existingOutcome == 562)
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' (Existing HOut = '{existingOutcome}'");

                return;
            }

            if (NisraRecordHasAlreadyBeenProcessed(nisraDataRecord, nisraOutcome, existingDataRecord, existingOutcome))
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' as is has already been updated on a previous run");

                return;
            }

            if (_blaiseApi.CaseInUseInCati(existingDataRecord))
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' as the case may be open in Cati");

                return;
            }

            if (existingOutcome > 0 && existingOutcome < nisraOutcome)
            {
                _loggingService.LogInfo(
                    $"Not processed: NISRA case '{primaryKey}' (Existing HOut = '{existingOutcome}' < '{nisraOutcome}')'");

                return;
            }

            UpdateCase(nisraDataRecord, existingDataRecord, instrumentName,
                serverParkName, nisraOutcome, existingOutcome, primaryKey);
        }

        internal bool NisraRecordHasAlreadyBeenProcessed(IDataRecord nisraDataRecord, int nisraOutcome,
            IDataRecord existingDataRecord, int existingOutcome)
        {
            return nisraOutcome == existingOutcome &&
                   _blaiseApi.GetLastUpdatedDateTime(nisraDataRecord) == _blaiseApi.GetLastUpdatedDateTime(existingDataRecord);
        }

        internal void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string instrumentName,
            string serverParkName, int newOutcome, int existingOutcome, string primaryKey)
        {
            var fieldData = GetFieldData(newDataRecord, existingDataRecord, newOutcome);

            _blaiseApi.UpdateCase(existingDataRecord, fieldData, instrumentName, serverParkName);

            if (RecordHasBeenUpdated(primaryKey, newDataRecord, newOutcome, instrumentName, serverParkName))
            {
                _loggingService.LogInfo(
                    $"processed: NISRA case '{primaryKey}' (NISRA HOut = '{newOutcome}' <= '{existingOutcome}') or (Existing HOut = 0)'");
                
                return;
            }

            _loggingService.LogWarn($"NISRA case '{primaryKey}' failed to update - potentially open in Cati at the time of the update");
        }

        internal Dictionary<string, string> GetFieldData(IDataRecord newDataRecord, IDataRecord existingDataRecord, int outcomeCode)
        {
            var newFieldData = _blaiseApi.GetRecordDataFields(newDataRecord);
            var existingFieldData = _blaiseApi.GetRecordDataFields(existingDataRecord);

            // we need to preserve the TO CatiMana block data sp remove the fields from WEB
            _catiDataService.RemoveCatiManaBlock(newFieldData);

            // we need to preserve the TO CallHistory block data captured in Cati
            _catiDataService.RemoveCallHistoryBlock(newFieldData);

            //we need to preserve the web nudged field
            _catiDataService.RemoveWebNudgedField(newFieldData);

            // add the existing cati call data with additional items to the new field data
            _catiDataService.AddCatiManaCallItems(newFieldData, existingFieldData, outcomeCode);

            return newFieldData;
        }

        internal bool RecordHasBeenUpdated(string primaryKey, IDataRecord newDataRecord, int newOutcomeCode,
            string instrumentName, string serverParkName)
        {
            var existingRecord = _blaiseApi.GetCase(primaryKey, instrumentName, serverParkName);

            return _blaiseApi.GetOutcomeCode(existingRecord) == newOutcomeCode &&
                   _blaiseApi.GetLastUpdatedDateTime(existingRecord) == _blaiseApi.GetLastUpdatedDateTime(newDataRecord);
        }
    }
}
