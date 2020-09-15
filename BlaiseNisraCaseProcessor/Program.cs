namespace BlaiseNisraCaseProcessor
{
    static class Program
    {
        // Instantiate logger.
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main()
        {
#if DEBUG
            Logger.Info("Blaise NISRA Case Processor service starting in DEBUG mode.");
            BlaiseNisraCaseProcessor blaiseNisraCaseProcessor = new BlaiseNisraCaseProcessor();
            blaiseNisraCaseProcessor.OnDebug();
#else
            Logger.Info("Blaise NISRA Case Processor service starting in RELEASE mode.");
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
