using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MicrowaveMonitor.Analysers
{
    public abstract class Analyser
    {
        public struct PercentDiff
        {
            public double Signal { get; set; }
            public double SignalQ { get; set; }
            public double TempIdu { get; set; }
            public double Voltage { get; set; }
            public double Latency { get; set; }
        }

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value != _isRunning)
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
        }
        public TimeSpan RefreshInterval { get; set; }
        public PercentDiff Percentages { get; set; }

        public static Dictionary<int, bool> WatchSignal = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchSignalQ = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempOdu = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempIduIn = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchTempIduOut = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchVoltage = new Dictionary<int, bool>();
        public static Dictionary<int, bool> WatchPing = new Dictionary<int, bool>();
        public static readonly object watchLocker = new object();

        protected readonly Dictionary<int, int> idsSignal = new Dictionary<int, int>();
        protected readonly Dictionary<int, int> idsSignalQ = new Dictionary<int, int>();
        protected readonly Dictionary<int, int> idsTempIdu = new Dictionary<int, int>();
        protected readonly Dictionary<int, int> idsVoltage = new Dictionary<int, int>();
        protected readonly Dictionary<int, int> idsPing = new Dictionary<int, int>();
        protected readonly object idsLocker = new object();

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

        public abstract void DeviceStopped(int devId);
    }
}
