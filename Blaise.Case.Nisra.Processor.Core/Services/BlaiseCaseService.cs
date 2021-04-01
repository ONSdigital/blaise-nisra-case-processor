using System.Runtime.CompilerServices;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;
using StatNeth.Blaise.API.DataRecord;

[assembly: InternalsVisibleTo("Blaise.Case.Nisra.Processor.Tests.Unit")]
namespace Blaise.Case.Nisra.Processor.Core.Services
{
    public class BlaiseCaseService : IBlaiseCaseService
    {
        private readonly IBlaiseCaseApi _blaiseApi;
        private readonly IFieldDataService _fieldDataService;
        private readonly ILoggingService _loggingService;

        public BlaiseCaseService(
            IBlaiseCaseApi blaiseApi,
            IFieldDataService fieldDataService,
            ILoggingService loggingService)
        {
            _blaiseApi = blaiseApi;
            _fieldDataService = fieldDataService;
            _loggingService = loggingService;
        }

        public void CreateCase(IDataRecord dataRecord, string instrumentName, string serverParkName,
            string primaryKey)
        {
            var newFieldData = _fieldDataService.GetNewCaseFieldData(dataRecord);

            _blaiseApi.CreateCase(primaryKey, newFieldData, instrumentName, serverParkName);
            _loggingService.LogInfo($"Created new Nisra case with primary '{primaryKey}'");
        }

        public void UpdateCase(IDataRecord newDataRecord, IDataRecord existingDataRecord, string instrumentName,
            string serverParkName, string primaryKey)
        {
            var fieldData = _fieldDataService.GetUpdateCaseFieldData(newDataRecord, existingDataRecord);

            _blaiseApi.UpdateCase(existingDataRecord, fieldData, instrumentName, serverParkName);

            if (RecordHasBeenUpdated(primaryKey, newDataRecord, instrumentName, serverParkName))
            {
                return;
            }

            _loggingService.LogWarn($"NISRA case '{primaryKey}' failed to update - potentially open in Cati at the time of the update for instrument '{instrumentName}'");
        }

        internal bool RecordHasBeenUpdated(string primaryKey, IDataRecord newDataRecord, string instrumentName, string serverParkName)
        {
            var existingRecord = _blaiseApi.GetCase(primaryKey, instrumentName, serverParkName);

            return _blaiseApi.GetOutcomeCode(existingRecord) == _blaiseApi.GetOutcomeCode(newDataRecord) &&
                   _blaiseApi.GetLastUpdatedDateTime(existingRecord) == _blaiseApi.GetLastUpdatedDateTime(newDataRecord);
        }
    }
}
