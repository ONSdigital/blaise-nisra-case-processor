using System.ServiceProcess;
using Blaise.Case.Nisra.Processor.Interfaces.Services;
using Blaise.Case.Nisra.Processor.Providers;

namespace Blaise.Case.Nisra.Processor
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
