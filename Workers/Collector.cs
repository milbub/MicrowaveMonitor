using MicrowaveMonitor.Database;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;

namespace MicrowaveMonitor.Workers
{
    public abstract class Collector
    {
        private const int _maxTimeout = 5000;   // ms
        private const int _avgAge = 60;         // s
        private const int _diffAge = 30;        // s
        private const int _avgSleep = 90;       // s

        protected int _deviceId;
        protected int _refreshInterval;
        protected IPAddress _address;
        protected bool _isRunning = false;

        protected DeviceDisplay _display;

        protected Thread tCollector;

        public static int MaxTimeout { get => _maxTimeout; }
        public static int AvgAge { get => _avgAge; }
        public static int DiffAge { get => _diffAge; }
        public static int AvgSleep { get => _avgSleep; }

        public int DeviceId { get => _deviceId; }
        public int RefreshInterval { get => _refreshInterval; }
        public IPAddress Address { get => _address; }
        public bool IsRunning { get => _isRunning; }

        public DeviceDisplay Display { get => _display; }

        public Collector(string address, int deviceId, int refreshInterval, DeviceDisplay display)
        {
            _address = IPAddress.Parse(address);
            _deviceId = deviceId;
            _refreshInterval = refreshInterval;
            _display = display;
        }

        public abstract void Start();

        public void Stop()
        {
            _isRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }

        public void Average()
        {
            //TimeSpan timediff = new TimeSpan(AvgAge * 10000000);
            //DateTime old = DateTime.Now - timediff;
            //double sum = 0;
            //int i;

            //for (i = 0; i < CollectedData.Count; i++)
            //{
            //    if (CollectedData[i].TimeMark < old)
            //        sum += CollectedData[i].Data;
            //    else
            //        break;
            //}

            //RecordAvg(sum / i);
        }

        public void Diff()
        {
            //TimeSpan timediff = new TimeSpan(DiffAge * 10000000);
            //DateTime old = DateTime.Now - timediff;
            //double sum = 0;
            //int x = 0;

            //for (int i = (CollectedData.Count - 1); i >= 0; i--)
            //{
            //    if (CollectedData[i].TimeMark > old)
            //    {
            //        sum += CollectedData[i].Data;
            //        x += 1;
            //    }
            //    else
            //        break;
            //}

            //RecordDiff(sum, x);
        }

        public virtual void RecordAvg(double avg)
        {
            throw new InvalidOperationException();
        }

        public virtual void RecordDiff(double sum, int count)
        {
            throw new InvalidOperationException();
        }
    }
}
