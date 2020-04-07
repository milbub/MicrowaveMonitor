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
        public static int MaxTimeout { get; private set; } = 5000;   // ms
        public int DeviceId { get; protected set; }
        public int RefreshInterval { get; protected set; }
        public IPAddress Address { get; protected set; }
        public bool IsRunning { get; protected set; } = false;
        public DeviceDisplay Display { get; protected set; }

        protected Thread tCollector;

        public Collector(string address, int deviceId, int refreshInterval, DeviceDisplay display)
        {
            Address = IPAddress.Parse(address);
            DeviceId = deviceId;
            RefreshInterval = refreshInterval;
            Display = display;
        }

        public abstract void Start();

        public void Stop()
        {
            IsRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }
    }
}
