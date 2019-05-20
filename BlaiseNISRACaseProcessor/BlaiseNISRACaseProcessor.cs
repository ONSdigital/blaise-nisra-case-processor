using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BlaiseNISRACaseProcessor
{
    public partial class BlaiseNISRACaseProcessor: ServiceBase
    {
        // Instantiate logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BlaiseNISRACaseProcessor()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Blaise NISRA Case Processor service started.");
            Run();
        }

        public void Run()
        {
            log.Info("----- RUNNING -----");
        }

        protected override void OnStop()
        {
            log.Info("Blaise NISRA Case Processor service stopped.");
        }
    }
}
