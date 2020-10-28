using System.ServiceProcess;
using Nisra.Case.Processor.Interfaces.Services;
using Nisra.Case.Processor.Providers;

namespace Nisra.Case.Processor
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
            InitialiseService.Start();
        }
        protected override void OnStop()
        {
            InitialiseService.Stop();
        }
    }
}
