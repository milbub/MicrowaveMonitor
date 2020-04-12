using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Database;
using System.Windows;

namespace MicrowaveMonitor
{
    public partial class App : Application
    {
        private readonly LinkManager linkManager;
        private readonly DataManager dataManager;
        private readonly WorkerManager workerManager;
        private readonly AlarmManager alarmManager;

        public App()
        {
            linkManager = new LinkManager();
            dataManager = new DataManager();
            workerManager = new WorkerManager(dataManager, linkManager);
            alarmManager = new AlarmManager();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            workerManager.InitWorkers(linkManager.GetDeviceTable());
            dataManager.StartDatabaseWriter();
            alarmManager.InitWatchers(workerManager.DeviceToFront);

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, alarmManager, dataManager);
            monitoringWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            workerManager.PauseWorkers();
            dataManager.StopDatabaseWriter();
            alarmManager.StopWatchers();
        }
    }
}
