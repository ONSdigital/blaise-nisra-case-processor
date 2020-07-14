using Blaise.Nuget.Api.Contracts.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Services.Blaise;
using BlaiseNisraCaseProcessor.Interfaces.Services.Data;
using log4net;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Services.Data
{
    public class UpdateRecordByWebFormStatusService : IUpdateRecordByWebFormStatusService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;
        private readonly IUpdateDataRecordByHoutService _updateByHoutService;

        public UpdateRecordByWebFormStatusService(
            ILog logger,
            IBlaiseApiService blaiseApiService, 
            IUpdateDataRecordByHoutService updateByHoutService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
            _updateByHoutService = updateByHoutService;
        }

        public void UpdateDataRecordViaWebFormStatus(IDataRecord newDataRecord, IDataRecord existingDataRecord, string serverPark,
            string surveyName, string serialNumber)
        {
            var newWebFormStatus = _blaiseApiService.GetWebFormStatus(newDataRecord);

            if (newWebFormStatus == WebFormStatusType.NotProcessed)
            {
                _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the Web form status is set as not processed");
                return;
            }

            var existingWebFormStatus = _blaiseApiService.GetWebFormStatus(existingDataRecord);

            if (existingWebFormStatus == WebFormStatusType.Complete)
            {
                if (newWebFormStatus == WebFormStatusType.Complete)
                {
                    _logger.Info($"The NISRA file for serial number '{serialNumber}' will be updated by HOut value as the Web form status is set as complete in both the existing record and NISRA file");
                    _updateByHoutService.UpdateDataRecordByHoutValues(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                    return;
                }

                _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the Web form status is set as complete for the existing but not in the NISRA file");
                return;
            }

            if (existingWebFormStatus == WebFormStatusType.Partial)
            {
                if (newWebFormStatus == WebFormStatusType.Partial)
                {
                    _logger.Info($"The NISRA file for serial number '{serialNumber}' will be updated by HOut value as the Web form status is set as partial in both the existing record and NISRA file");
                    _updateByHoutService.UpdateDataRecordByHoutValues(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                    return;
                }

                _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the Web form status is set as complete for the existing but not in teh NISRA file");
                _blaiseApiService.UpdateDataRecord(newDataRecord, existingDataRecord, serverPark, surveyName);
                return;
            }

            if (newWebFormStatus != WebFormStatusType.NotSpecified)
            {
                _logger.Info($"The NISRA file for serial number '{serialNumber}' will be updated by HOut value as the Web form status of the NISRA file is '{newWebFormStatus}'");
                _updateByHoutService.UpdateDataRecordByHoutValues(newDataRecord, existingDataRecord, serverPark, surveyName, serialNumber);
                return;
            }

            _logger.Info($"The NISRA file has not been processed for serial number '{serialNumber}' as the Web form status of the NISRA file is '{newWebFormStatus}'");
        }
    }
}
