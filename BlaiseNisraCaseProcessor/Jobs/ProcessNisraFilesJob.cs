using System;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using log4net;
using Quartz;

namespace BlaiseNisraCaseProcessor.Jobs
{
    public class ProcessNisraFilesJob : IJob
    {
        private readonly ILog _logger;
        private readonly IProcessNisraFilesService _processNisraFilesService;

        public ProcessNisraFilesJob(
            ILog logger, 
            IProcessNisraFilesService processNisraFilesService)
        {
            _logger = logger;
            _processNisraFilesService = processNisraFilesService;
        }

        public void Execute(IJobExecutionContext context)
        {
            _logger.Info($"Executing job 'DownloadAndProcessAvailableFiles' at '{DateTime.Now.TimeOfDay}' on '{DateTime.Now.Date}'");
            _processNisraFilesService.DownloadAndProcessAvailableFiles();
        }
    }
}
