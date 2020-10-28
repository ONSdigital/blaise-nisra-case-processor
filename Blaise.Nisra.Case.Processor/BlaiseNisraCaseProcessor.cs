using System.ServiceProcess;
using Blaise.Nisra.Case.Processor.Interfaces.Services;
using Blaise.Nisra.Case.Processor.Providers;

namespace Blaise.Nisra.Case.Processor
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
