using System;
using System.Linq;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;

namespace BlaiseNisraCaseProcessor.MessageHandler
{
    public class NisraMessageHandler : IMessageHandler
    {
        private readonly ILog _logger;
        private readonly ICloudBucketFileService _bucketFileService;
        private readonly IProcessFilesService _processNisraFilesService;

        public NisraMessageHandler(
            ILog logger,
            ICloudBucketFileService bucketFileService,
            IProcessFilesService processNisraFilesService)
        {
            _bucketFileService = bucketFileService;
            _processNisraFilesService = processNisraFilesService;
            _logger = logger;
        }


        public bool HandleMessage(string message)
        {
            try
            {
                _logger.Info($"Message received '{message}'");

                var availableFiles = _bucketFileService.GetFilesFromBucket().ToList();

                if (!availableFiles.Any())
                {
                    _logger.Info("No available files found in the bucket");
                    return true;
                }

                _processNisraFilesService.ProcessFiles(availableFiles);

                _logger.Info($"Message processed '{message}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Info($"Error processing message '{message}', with inner exception {ex.InnerException}");

                return false;
            }
        }
    }
}
