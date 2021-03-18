using System.ServiceProcess;
using Blaise.Case.Nisra.Processor.WindowsService.Interfaces;
using Blaise.Case.Nisra.Processor.WindowsService.Ioc;

namespace Blaise.Case.Nisra.Processor.WindowsService
{
    public partial class BlaiseNisraCaseProcessor : ServiceBase
    {
        public IInitialiseWindowsService InitialiseService;

        public BlaiseNisraCaseProcessor()
        {
            InitializeComponent();
            var unityProvider = new UnityProvider();

            InitialiseService = unityProvider.Resolve<IInitialiseWindowsService>();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            InitialiseService.Start();
        }
        protected override void OnStop()
        {
            InitialiseService.Stop();
        }
    }
}
