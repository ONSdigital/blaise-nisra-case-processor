using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Services
{
    public class UpdateCaseService : IUpdateCaseService
    {
        private readonly ILog _logger;
        private readonly IBlaiseApiService _blaiseApiService;

        public UpdateCaseService(
            ILog logger,
            IBlaiseApiService blaiseApiService)
        {
            _logger = logger;
            _blaiseApiService = blaiseApiService;
        }

        public void UpdateCase(IDataRecord nisraDataRecord, IDataRecord existingDataRecord, string serverPark, string surveyName, string serialNumber)
        {
            var nisraOutcome = _blaiseApiService.GetHOutValue(nisraDataRecord);

            if (nisraOutcome == 0)
            {
                _logger.Info($"Not processed: NISRA case-serial-number '{serialNumber}' (HOut = 0)");
                return;
            }

            var existingOutcome = _blaiseApiService.GetHOutValue(existingDataRecord);

            if (existingOutcome > 0 && existingOutcome < nisraOutcome)
            {
                _logger.Info($"Not processed: NISRA case-serial-number '{serialNumber}' (HOut = '{existingOutcome}' < '{nisraOutcome}')'");

                return;
            }

            _blaiseApiService.UpdateCase(nisraDataRecord, existingDataRecord, serverPark, surveyName);
            _logger.Info($"processed: NISRA case-serial-number '{serialNumber}' (HOut = '{existingOutcome}' > '{nisraOutcome}') or (HOut = 0)'");
        }
    }
}
