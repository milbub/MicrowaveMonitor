using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;

namespace MicrowaveMonitor.Managers
{
    public class AlarmManager
    {
        private const int refresh = 5000;
        private const double diffLimit = 0.1;
        private static TimeSpan notRespondLimit = new TimeSpan(100000000);

        private ObservableCollection<string> _alarms = new ObservableCollection<string>();
        private Dictionary<int, DeviceDisplay> _deviceStats;
        private bool _isRunning;
        private DateTime startTime;
        private Thread tWatcher;

        public ObservableCollection<string> Alarms { get => _alarms; }
        public Dictionary<int, DeviceDisplay> DeviceStats { get => _deviceStats; }
        public bool IsRunning { get => _isRunning; }

        public AlarmManager()
        { }

        public void InitWatchers(Dictionary<int, DeviceDisplay> deviceToFront)
        {
            _deviceStats = deviceToFront;
            startTime = DateTime.Now;
            Watch();
        }

        public void StopWatchers()
        {
            _isRunning = false;
            tWatcher.Abort();
        }

        private void Watch()
        {
            _isRunning = true;
            tWatcher = new Thread(() =>
            {
                while (_isRunning)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _alarms.Clear();
                    });
                    foreach (KeyValuePair<int, DeviceDisplay> item in DeviceStats)
                    {
                        //DiffAlarm(item.Value.AvgPing, item.Value.DiffPing, item.Key, "ms", "ping");
                        //DiffAlarm(item.Value.AvgSig, item.Value.DiffSig, item.Key, "dBm", "signal");
                        //DiffAlarm(item.Value.AvgSigQ, item.Value.DiffSigQ, item.Key, "dB", "signal quality");
                        //notRespondAlarm(item.DataSignal, item.Address);
                    }

                    Thread.Sleep(refresh);
                }
            });
            tWatcher.Start();
        }

        private void DiffAlarm(double avg, double diff, int id, string units, string meter)
        {
            double maxdiff = avg * diffLimit;
            double maxval = avg + diff;
            if (diff > maxdiff)
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _alarms.Add(String.Format("Device {0} has exceeded maximum ({1} {2}) {3} treshold!", id, maxval, units, meter));
                });
        }

        //private void notRespondAlarm(ObservableCollection<RecordDouble> collection, IPAddress address)
        //{
        //    if (collection.Count == 0)
        //    {
        //        if ((DateTime.Now - startTime) > notRespondLimit)
        //            App.Current.Dispatcher.Invoke((Action)delegate
        //            {
        //                _alarms.Add(String.Format("Device {0} is not responding since the start of monitoring!", address));
        //            });
        //    }
        //    else
        //    {
        //        DateTime old = DateTime.Now - notRespondLimit;
        //        if (collection.Last().TimeMark < old)
        //        {
        //            TimeSpan time = DateTime.Now - collection.Last().TimeMark;
        //            App.Current.Dispatcher.Invoke((Action)delegate
        //            {
        //                _alarms.Add(String.Format("Device {0} is not responding for {1} seconds!", address, time.TotalSeconds));
        //            });
        //        }
        //    }
        //}
    }
}
