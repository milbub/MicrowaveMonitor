using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MicrowaveMonitor.Managers;

namespace MicrowaveMonitor
{
    public partial class App : Application
    {
        private LinkManager linkManager = new LinkManager();
        private WorkerManager workerManager = new WorkerManager();
        private IncidentManager incidentManager = new IncidentManager();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            linkManager.LoadLinks();
            workerManager.InitWorkers(linkManager.LinkDatabase);
            incidentManager.StartWatchers(linkManager.LinkDatabase);

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, incidentManager);
            monitoringWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            workerManager.StopWorkers(linkManager.LinkDatabase);
        }
    }
}
