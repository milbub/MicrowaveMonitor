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
        private ObservableCollection<string> _alarms;
        private bool _isRunning;
        private List<Device> monitored;
        private DateTime startTime;
        private Thread tWatcher;
        private const int refresh = 5000;
        private const double diffLimit = 0.1;
        private static TimeSpan notRespondLimit = new TimeSpan(100000000);

        public ObservableCollection<string> Alarms { get => _alarms; }
        public bool IsRunning { get => _isRunning; }

        public AlarmManager()
        {
            _alarms = new ObservableCollection<string>();
            monitored = new List<Device>();
        }

        public void InitWatchers(Dictionary<string, Link> linkDatabase)
        {
            foreach (Link link in linkDatabase.Values)
            {
                switch (link.HopCount)
                {
                    case 0:
                        monitored.Add(link.BaseDevice);
                        break;
                    case 1:
                        monitored.Add(link.EndDevice);
                        goto case 0;
                    case 2:
                        monitored.Add(link.RelayOne);
                        goto case 1;
                    case 3:
                        monitored.Add(link.RelayTwo);
                        goto case 2;
                    case 4:
                        monitored.Add(link.RelayThree);
                        goto case 3;
                    case 5:
                        monitored.Add(link.RelayFour);
                        goto case 4;
                    default:
                        throw new NotSupportedException();
                }
            }
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
                    foreach (Device item in monitored)
                    {
                        DiffAlarm(item.AvgPing, item.DiffPing, item.Address.Address, "ms", "ping");
                        DiffAlarm(item.AvgSig, item.DiffSig, item.Address.Address, "dBm", "signal");
                        DiffAlarm(item.AvgSigQ, item.DiffSigQ, item.Address.Address, "dB", "signal quality");
                        notRespondAlarm(item.DataSignal, item.Address.Address);
                    }
                    Thread.Sleep(refresh);
                }
            });
            tWatcher.Start();
        }

        private void DiffAlarm(double avg, double diff, IPAddress address, string units, string meter)
        {
            double maxdiff = avg * diffLimit;
            double maxval = avg + diff;
            if (diff > maxdiff)
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _alarms.Add(String.Format("Device {0} has exceeded maximum ({1} {2}) {3} treshold!", address, maxval, units, meter));
                });
        }

        private void notRespondAlarm(ObservableCollection<RecordDouble> collection, IPAddress address)
        {
            if (collection.Count == 0)
            {
                if ((DateTime.Now - startTime) > notRespondLimit)
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _alarms.Add(String.Format("Device {0} is not responding since the start of monitoring!", address));
                    });
            }
            else
            {
                DateTime old = DateTime.Now - notRespondLimit;
                if (collection.Last().TimeMark < old)
                {
                    TimeSpan time = DateTime.Now - collection.Last().TimeMark;
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _alarms.Add(String.Format("Device {0} is not responding for {1} seconds!", address, time.TotalSeconds));
                    });
                }
            }
        }
    }
}
