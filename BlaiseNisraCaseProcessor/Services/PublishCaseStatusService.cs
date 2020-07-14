using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;
using StatNeth.Blaise.API.DataRecord;

namespace BlaiseNisraCaseProcessor.Services
{
    public class PublishCaseStatusService : IPublishCaseStatusService
    {
        private readonly ILog _logger;
        private readonly IQueueService _queueService;
        private readonly ICaseMapper _mapper;

        public PublishCaseStatusService(
            ILog logger,
            IQueueService queueService,
            ICaseMapper mapper)
        {
            _logger = logger;
            _queueService = queueService;
            _mapper = mapper;
        }

        public void PublishCaseStatus(IDataRecord recordData, string instrumentName, string serverPark, string primaryKey, CaseStatusType caseStatusType)
        {
            var message = _mapper.MapToSerializedJson(recordData, instrumentName, serverPark, primaryKey, caseStatusType);

            _queueService.PublishMessage(message);

            _logger.Info($"Message '{message}' was published");
        }
    }
}
