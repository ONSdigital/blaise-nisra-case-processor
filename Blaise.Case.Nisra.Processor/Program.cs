using System.ServiceProcess;

namespace Blaise.Case.Nisra.Processor.WindowsService
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            BlaiseNisraCaseProcessor blaiseNisraCaseProcessor = new BlaiseNisraCaseProcessor();
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
