using MicrowaveMonitor.Managers;
using System.Windows;

namespace MicrowaveMonitor
{
    public partial class App : Application
    {
        private LinkManager linkManager = new LinkManager();
        private WorkerManager workerManager = new WorkerManager();
        private AlarmManager alarmManager = new AlarmManager();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            linkManager.LoadLinks();
            workerManager.InitWorkers(linkManager.LinkDatabase);
            alarmManager.StartWatcher(linkManager.LinkDatabase);

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, alarmManager);
            monitoringWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            workerManager.StopWorkers(linkManager.LinkDatabase);
            alarmManager.StopWatcher();
        }
    }
}
