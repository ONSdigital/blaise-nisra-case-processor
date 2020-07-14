using System;
using System.Linq;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Interfaces.Services.Files;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class ProcessNisraFilesService : IProcessNisraFilesService
    {
        private readonly ILog _logger;
        private readonly ICloudBucketFileService _bucketFileService;
        private readonly IProcessFilesService _processNisraFilesService;

        public ProcessNisraFilesService(
            ILog logger,
            ICloudBucketFileService bucketFileService,
            IProcessFilesService processNisraFilesService)
        {
            _bucketFileService = bucketFileService;
            _processNisraFilesService = processNisraFilesService;
            _logger = logger;
        }

        public void DownloadAndProcessAvailableFiles()
        {
            try
            {
                var availableFiles = _bucketFileService.GetFilesFromBucket().ToList();

                if (!availableFiles.Any())
                {
                    _logger.Info("No available files found in the bucket");
                    return;
                }

                _processNisraFilesService.ProcessFiles(availableFiles);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occured trying to download and process NISRA files '{e.Message}'");
            }
        }
    }
}
