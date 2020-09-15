using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;
using StatNeth.Blaise.API.DataRecord;
using CaseStatusType = BlaiseNisraCaseProcessor.Enums.CaseStatusType;

namespace BlaiseNisraCaseProcessor.Services
{
    public class UpdateCaseService : IUpdateCaseService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IPublishCaseStatusService _publishCaseStatusService;

        public UpdateCaseService(
            ILog logger,
            IBlaiseApiService blaiseApiService,
            IPublishCaseStatusService publishCaseStatusService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _publishCaseStatusService = publishCaseStatusService;
        }

        public void UpdateCase(IDataRecord nisraDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName, string serialNumber)
        {
            var nisraOutcome = _blaiseApiService.GetHOutValue(nisraDataRecord);

            if (nisraOutcome == 0)
            {
                _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the and the HOut value is '{nisraOutcome}' which indicates it has not been processed");
                return;
            }

            var existingOutcome = _blaiseApiService.GetHOutValue(existingDataRecord);

            if (existingOutcome > 0 && existingOutcome < nisraOutcome)
            {
                _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the existing HOut '{existingOutcome}' is better than the NISRA HOut '{nisraOutcome}'");

                return;
            }

            ImportNisraCase(nisraDataRecord, existingDataRecord, serverPark, surveyName);
            _logger.Info($"The NISRA file has been imported for serial number '{serialNumber}' as the HOut value '{nisraOutcome}' is the same in both the NISRA file and the database");
        }

        private void ImportNisraCase(IDataRecord nisraDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName)
        {
            _blaiseApiService.UpdateCase(nisraDataRecord, existingDataRecord, serverPark, surveyName);
            _publishCaseStatusService.PublishCaseStatus(nisraDataRecord, surveyName, serverPark, CaseStatusType.NisraCaseImported);
        }
    }
}
