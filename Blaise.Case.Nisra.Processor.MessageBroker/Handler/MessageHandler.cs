using System;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.Logging.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Nisra.Processor.MessageBroker.Handler
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILoggingService _loggingService;
        private readonly IStorageService _storageService;
        private readonly IImportNisraDataService _nisraDataService;
        private readonly IMessageModelMapper _mapper;

        public MessageHandler(
            ILoggingService loggingService,
            IStorageService storageService,
            IImportNisraDataService nisraDataService, 
            IMessageModelMapper mapper)
        {
            _loggingService = loggingService;
            _storageService = storageService;
            _nisraDataService = nisraDataService;
            _mapper = mapper;
        }
        
        public bool HandleMessage(string message)
        {
            try
            {
                _loggingService.LogInfo($"Message '{message}' received at '{DateTime.Now}'");

                var messageModel = _mapper.MapToMessageModel(message);

                var databaseFile = _storageService.GetInstrumentFileFromBucket(
                    messageModel.InstrumentName, messageModel.InstrumentBucketPath);

                _nisraDataService.ImportNisraDatabaseFile(messageModel.ServerParkName, messageModel.InstrumentName, 
                    databaseFile);

                _storageService.DeleteDownloadedFiles();

                _loggingService.LogInfo($"Finished processing '{message}' at '{DateTime.Now}'");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error processing message '{message}' at {DateTime.Now}'", ex);

                return false;
            }
        }
    }
}
