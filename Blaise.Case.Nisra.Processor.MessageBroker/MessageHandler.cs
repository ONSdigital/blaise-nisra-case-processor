using System;
using System.Linq;
using Blaise.Case.Nisra.Processor.CloudStorage.Interfaces;
using Blaise.Case.Nisra.Processor.Core.Interfaces;
using Blaise.Case.Nisra.Processor.MessageBroker.Enums;
using Blaise.Case.Nisra.Processor.MessageBroker.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;

namespace Blaise.Case.Nisra.Processor.MessageBroker
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILog _logger;
        private readonly IStorageService _storageService;
        private readonly IProcessFilesService _processFilesService;
        private readonly IMessageModelMapper _mapper;

        public MessageHandler(
            ILog logger,
            IStorageService storageService,
            IProcessFilesService processNisraFilesService, 
            IMessageModelMapper mapper)
        {
            _storageService = storageService;
            _processFilesService = processNisraFilesService;
            _mapper = mapper;
            _logger = logger;
        }


        public bool HandleMessage(string message)
        {
            try
            {
                var messagedReceivedDateTime = DateTime.Now;
                _logger.Info($"Message received '{message}' at '{messagedReceivedDateTime}'");

                var messageModel = _mapper.MapToNisraCaseActionModel(message);

                if (messageModel.Action != ActionType.Process)
                {
                    _logger.Info("The Message '{message}' is not valid");
                    return true;
                }

                var availableFilesInBucket = _storageService.GetListOfAvailableFilesInBucket().ToList();

                if (!availableFilesInBucket.Any())
                {
                    _logger.Info("No available files found in the bucket");
                    return true;
                }

                var downloadedFilesFromBucket = _storageService.DownloadFilesFromBucket(availableFilesInBucket).ToList();

                if (!downloadedFilesFromBucket.Any())
                {
                    _logger.Info("No files were downloaded from the bucket");
                    return true;
                }

                _processFilesService.ProcessFiles(downloadedFilesFromBucket);

                _storageService.MoveProcessedFilesToProcessedFolder(availableFilesInBucket);

                _logger.Info($"Finished processing '{message}' received at '{messagedReceivedDateTime}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing message '{message}', with exception {ex}");

                return false;
            }
        }
    }
}
