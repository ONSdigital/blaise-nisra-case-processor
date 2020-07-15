using System.IO;
using System.ServiceProcess;
using BlaiseNisraCaseProcessor.Interfaces.Services;
using BlaiseNisraCaseProcessor.Providers;

namespace BlaiseNisraCaseProcessor
{
    public partial class BlaiseNisraCaseProcessor : ServiceBase
    {
        public IInitialiseService InitialiseService;

        public BlaiseNisraCaseProcessor()
        {
            InitializeComponent();
            var unityProvider = new UnityProvider();

            InitialiseService = unityProvider.Resolve<IInitialiseService>();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            //InitialiseService.Start();
            var unityProvider = new UnityProvider();
            var processFilesService = unityProvider.Resolve<IProcessFilesService>();

            var files = Directory.GetFiles(@"D:\Temp\OPN\Nisra\opn2004a");

            processFilesService.ProcessFiles(files);
        }

        protected override void OnStop()
        {
            InitialiseService.Stop();
        }
    }
}
