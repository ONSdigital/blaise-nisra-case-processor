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
            InitialiseService.Start();
        }
        protected override void OnStop()
        {
            InitialiseService.Stop();
        }
    }
}
