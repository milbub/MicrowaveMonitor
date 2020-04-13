﻿using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Database;
using System;
using System.Windows;

namespace MicrowaveMonitor
{
    public partial class App : Application
    {
        private readonly LinkManager linkManager;
        private readonly DataManager dataManager;
        private readonly WorkerManager workerManager;
        private readonly AlarmManager alarmManager;
        private readonly LogManager logManager;

        public App()
        {
            logManager = new LogManager(Console.Out);           
            Console.SetOut(logManager);
            Console.WriteLine("0Application started.");           
            linkManager = new LinkManager();
            dataManager = new DataManager();
            alarmManager = new AlarmManager();
            workerManager = new WorkerManager(dataManager, linkManager, alarmManager);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            workerManager.InitWorkers(linkManager.GetDeviceTable());
            dataManager.StartDatabaseWriter();
            alarmManager.InitWatchers(workerManager.DeviceToFront);

            MonitoringWindow monitoringWindow = new MonitoringWindow(linkManager, workerManager, alarmManager, dataManager);
            monitoringWindow.Show();

            logManager.SetGuiLog(monitoringWindow.eventLog);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            logManager.IsExiting = true;
            workerManager.PauseWorkers();
            dataManager.IsRunning = false;
            alarmManager.IsRunning = false;
        }
    }
}
