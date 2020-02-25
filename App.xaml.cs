using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Database;
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
            //linkManager.LoadLinks();
            workerManager.InitWorkers(linkManager.LinkDatabase.Table<Device>());
            alarmManager.InitWatchers(linkManager.LinkDatabase.Table<Device>());

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, alarmManager);
            monitoringWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            workerManager.StopWorkers(linkManager.LinkDatabase.Table<Device>());
            alarmManager.StopWatchers();
        }
    }
}
