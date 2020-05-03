using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Database;
using System;
using System.Windows;
using System.Collections.Generic;

namespace MicrowaveMonitor
{
    public partial class App : Application
    {
        private readonly LinkManager linkManager;
        private readonly DataManager dataManager;
        private readonly WorkerManager workerManager;
        private readonly AlarmManager alarmManager;
        private readonly LogManager logManager;
        
        private readonly Dictionary<int, DeviceDisplay> deviceDisplays;

        public App()
        {
            logManager = new LogManager(Console.Out);
            
            Console.SetOut(logManager);
            Console.WriteLine("0Application started.");

            linkManager = new LinkManager();
            dataManager = new DataManager();
            deviceDisplays = new Dictionary<int, DeviceDisplay>();
            alarmManager = new AlarmManager(dataManager, linkManager, deviceDisplays);
            workerManager = new WorkerManager(dataManager, linkManager, alarmManager, deviceDisplays);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            workerManager.InitWorkers(linkManager.GetDeviceTable());
            dataManager.IsRunning = true;
            alarmManager.IsRunning = true;

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, alarmManager, dataManager);
            monitoringWindow.Show();

            logManager.SetGuiLog(monitoringWindow.eventLog);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            logManager.IsExiting = true;
            dataManager.IsRunning = false;
            alarmManager.IsRunning = false;
        }
    }
}
