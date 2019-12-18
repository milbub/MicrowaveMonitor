using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    public abstract class Collector
    {
        protected Device _device;
        protected bool _isRunning = false;
        const int _maxTimeout = 5000;

        public Device Device { get => _device; }
        public bool IsRunning { get => _isRunning; }
        static public int MaxTimeout { get => _maxTimeout; }

        public Collector(Device device)
        {
            _device = device;
        }

        public virtual void Stop()
        {
            _isRunning = false;
        }
    }
}
