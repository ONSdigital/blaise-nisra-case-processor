using System.ServiceProcess;

namespace Blaise.Case.Nisra.Processor.WindowsService
{
    internal static class Program
    {
        private static void Main()
        {
#if DEBUG
            var blaiseNisraCaseProcessor = new BlaiseNisraCaseProcessor();
            blaiseNisraCaseProcessor.OnDebug();
#else
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
