using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Services
{
    public class UpdateDataRecordByHoutService : IUpdateDataRecordByHoutService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IPublishCaseStatusService _publishCaseStatusService;

        public UpdateDataRecordByHoutService(
            ILog logger, 
            IBlaiseApiService blaiseApiService, 
            IPublishCaseStatusService publishCaseStatusService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _publishCaseStatusService = publishCaseStatusService;
        }

        public void UpdateDataRecordByHoutValues(IDataRecord newDataRecord, IDataRecord existingDataRecord,
            string serverPark, string surveyName, string serialNumber)
        {
            if (!_blaiseApiService.HOutFieldExists(newDataRecord))
            {
                _logger.Info($"HOut field did not exist in the NISRA file for serial number '{serialNumber}'");
                return;
            }

            var newHOut = _blaiseApiService.GetHOutValue(newDataRecord);

            if (newHOut == 0)
            {
                _logger.Info($"The NISRA file for serial number '{serialNumber}' has not been processed as the HOut value is '{newHOut}'");
                return;
            }

            var existingHOut = _blaiseApiService.GetHOutValue(existingDataRecord);

            if (newHOut < existingHOut || existingHOut == 0)
            {
                _blaiseApiService.UpdateDataRecord(newDataRecord, existingDataRecord, serverPark, surveyName);
                _logger.Info($"The NISRA file for serial number '{serialNumber}' has been updated as it has a better HOut ('{newHOut}') than the existing record ('{existingHOut}')");

                _publishCaseStatusService.PublishCaseStatus(newDataRecord, surveyName, serverPark, CaseStatusType.NisraCaseImported);
            }
        }
    }
}
