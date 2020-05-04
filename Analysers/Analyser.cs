using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Analysers
{
    public abstract class Analyser
    {
        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
        public abstract TimeSpan RefreshInterval { get; set; }

        public static Dictionary<int, bool> WatchSignal = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchSignalQ = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempOdu = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempIduIn = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempIduOut = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchVoltage = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchPing = new Dictionary<int, bool>();
        public static readonly object watchLocker = new object();

        protected readonly AlarmManager alarmMan;
        protected readonly DataManager dataMan;
        protected Thread tQueryer;

        public Analyser(AlarmManager alarmManager, DataManager dataManager)
        {
            alarmMan = alarmManager;
            dataMan = dataManager;
        }

        protected abstract void Start();

        protected virtual void Stop()
        {
            tQueryer.Abort();
        }
    }
}
