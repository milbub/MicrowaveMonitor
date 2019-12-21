using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    public abstract class Collector
    {
        protected Device _device;
        protected bool _isRunning = false;
        private const int _maxTimeout = 5000;

        public Device Device { get => _device; }
        public bool IsRunning { get => _isRunning; }
        public static int MaxTimeout { get => _maxTimeout; }

        public Collector(Device device)
        {
            _device = device;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
