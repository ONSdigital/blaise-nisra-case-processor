using System.ServiceProcess;

namespace BlaiseNisraCaseProcessor
{
    static class Program
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main()
        {
#if DEBUG
            log.Info("Blaise NISRA Case Processor service starting in DEBUG mode.");
            BlaiseNisraCaseProcessor bncpService = new BlaiseNisraCaseProcessor();
            bncpService.OnDebug();
#else
            log.Info("Blaise NISRA Case Processor service starting in RELEASE mode.");
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new BlaiseNisraCaseProcessor() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
