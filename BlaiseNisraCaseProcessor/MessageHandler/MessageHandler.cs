using System;
using System.Linq;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Enums;
using BlaiseNisraCaseProcessor.Interfaces.Mappers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.MessageHandler
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILog _logger;
        private readonly IBucketService _bucketFileService;
        private readonly IProcessFilesService _processFilesService;
        private readonly ICaseMapper _mapper;

        public MessageHandler(
            ILog logger,
            IBucketService bucketFileService,
            IProcessFilesService processNisraFilesService, 
            ICaseMapper mapper)
        {
            _bucketFileService = bucketFileService;
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

                var availableFilesInBucket = _bucketFileService.GetListOfAvailableFilesInBucket();

                if (!availableFilesInBucket.Any())
                {
                    _logger.Info("No available files found in the bucket");
                    return true;
                }

                var downloadedFilesFromBucket = _bucketFileService.DownloadFilesFromBucket(availableFilesInBucket).ToList();

                if (!downloadedFilesFromBucket.Any())
                {
                    _logger.Info("No files were downloaded from the bucket");
                    return true;
                }

                _processFilesService.ProcessFiles(downloadedFilesFromBucket);

                _bucketFileService.MoveProcessedFilesToProcessedFolder(availableFilesInBucket);

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
