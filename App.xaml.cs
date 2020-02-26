﻿using MicrowaveMonitor.Managers;
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
            workerManager.InitWorkers(linkManager.LinkDatabase.Table<Device>());
            alarmManager.InitWatchers(workerManager.DeviceToFront);

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, alarmManager);
            monitoringWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            workerManager.StopWorkers();
            alarmManager.StopWatchers();
        }
    }
}
