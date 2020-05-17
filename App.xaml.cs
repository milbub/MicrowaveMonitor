using MicrowaveMonitor.Database;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
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

        private readonly Dictionary<int, DeviceDisplay> deviceDisplays;

        private static readonly string appGuid = "a6c1a2ee-9836-01ea-bb37-0242ac130002";

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

        [STAThread]
        public static void Main()
        {
            using (Mutex mutex = new Mutex(true, appGuid, out bool isNotRunning))
            {
                if (isNotRunning)
                {
                    App application = new App();
                    application.InitializeComponent();
                    application.Run();
                }
                else
                {
                    MessageBox.Show("Microwave Monitor is already running.", "Startup Error");
                    return;
                }
            }
        }
    }
}
