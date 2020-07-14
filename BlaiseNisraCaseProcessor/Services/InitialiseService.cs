using System;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Interfaces.Services.Jobs;
using log4net;

namespace BlaiseNisraCaseProcessor.Services
{
    public class InitialiseService : IInitialiseService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IJobSchedulerService _jobSchedulerService;

        public InitialiseService(
            ILog logger, 
            IConfigurationProvider configurationProvider, 
            IJobSchedulerService jobSchedulerService)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _jobSchedulerService = jobSchedulerService;
        }

        public void Start()
        {
            _logger.Info($"Nisra Case processing service service on '{_configurationProvider.VmName}'");
          
            try
            {
                _jobSchedulerService.ScheduleJob();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            _logger.Info($"Nisra Case processing service started on '{_configurationProvider.VmName}'");
        }

        public void Stop()
        {
            _logger.Info($"Nisra Case processing service stopped on '{_configurationProvider.VmName}'");
        }
    }
}