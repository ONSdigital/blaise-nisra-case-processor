using System.Configuration;
using BlaiseNisraCaseProcessor.Interfaces.Providers;
using BlaiseNisraCaseProcessor.Interfaces.Services.Jobs;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace BlaiseNisraCaseProcessor.Services.Jobs
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILog _logger;
        private readonly IJob _job;
        private readonly IConfigurationProvider _configurationProvider;

        private static IScheduler _scheduler;

        public JobSchedulerService(
            ILog logger, 
            IJob job, 
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _job = job;
            _configurationProvider = configurationProvider;
        }

        public void ScheduleJob()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
            _scheduler.Start();
            AddJob();
        }

        public void AddJob()
        {
            var quartzCron = _configurationProvider.QuartzCron;
            var jobDetail = new JobDetailImpl("job", "group", _job.GetType());
            var triggerDetail = new CronTriggerImpl("trigger", "group", quartzCron);
            _scheduler.ScheduleJob(jobDetail, triggerDetail);

            _logger.Info($"Job scheduled using cron '{quartzCron}'");
        }
    }
}
