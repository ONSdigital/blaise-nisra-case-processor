using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BlaiseNISRACaseProcessor
{
    static class Program
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main()
        {
#if DEBUG
            log.Info("Blaise NISRA Case Processor service starting in DEBUG mode.");

            BlaiseNISRACaseProcessor bncpService = new BlaiseNISRACaseProcessor();
            bncpService.OnDebug();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new BlaiseNISRACaseProcessor() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
