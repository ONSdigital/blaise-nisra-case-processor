using Blaise.Case.Nisra.Processor.Interfaces.Services;
using log4net;
using StatNeth.Blaise.API.DataRecord;

namespace Blaise.Case.Nisra.Processor.Services
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
                _logger.Info($"Not processed: NISRA case '{serialNumber}' (HOut = 0)");

                return;
            }

            var existingOutcome = _blaiseApiService.GetHOutValue(existingDataRecord);

            if (existingOutcome > 542)
            {
                _logger.Info($"Not processed: NISRA case '{serialNumber}' (Existing HOut = '{existingOutcome}'");

                return;
            }

            if (existingOutcome == 0 || nisraOutcome <= existingOutcome)
            {
                _blaiseApiService.UpdateCase(nisraDataRecord, existingDataRecord, serverPark, surveyName);
                _logger.Info($"processed: NISRA case '{serialNumber}' (HOut = '{nisraOutcome}' <= '{existingOutcome}') or (HOut = 0)'");

                return;
            }

            _logger.Info($"Not processed: NISRA case '{serialNumber}' (HOut = '{existingOutcome}' < '{nisraOutcome}')'");
        }
    }
}
