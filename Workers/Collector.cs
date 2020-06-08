using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Models;
using System.Net;
using System.Threading;

namespace MicrowaveMonitor.Workers
{
    public abstract class Collector
    {
        public static int MaxTimeoutCount { get; set; } = 10;
        public static int MaxTimeout { get; private set; } = 5000;        // ms
        public static int MaxDownRefresh { get; private set; } = 30000;   // ms
        public int RefreshInterval { get; protected set; }

        public int DeviceId { get; protected set; }
        public IPAddress Address { get; protected set; }
        public DeviceDisplay Display { get; protected set; }

        public bool IsRunning { get; protected set; } = false;
        protected abstract Measurement MeasureType { get; }

        protected Thread tCollector;

        protected readonly AlarmManager alarmMan;

        protected readonly bool checkTresh;
        protected readonly float trUp;
        protected readonly float trDown;
        protected bool treshActive = false;
        protected bool treshOver = false;

        private int timeoutCount = 0;
        private int origRefresh;

        public Collector(string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown)
        {
            Address = IPAddress.Parse(address);
            DeviceId = deviceId;
            RefreshInterval = refreshInterval;
            Display = display;
            alarmMan = alarmManager;
            checkTresh = checkTresholds;
            trUp = treshUp;
            trDown = treshDown;
        }

        public abstract void Start();

        public void Stop()
        {
            IsRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }

        protected virtual void HasResponded(bool responded)
        {
            if (responded)
            {
                if (timeoutCount < MaxTimeoutCount)
                    timeoutCount = 0;
                else
                {
                    alarmMan.DeviceUpTrigger(DeviceId, false);
                    RefreshInterval = origRefresh;
                    timeoutCount = 0;
                }
            }
            else
            {
                timeoutCount++;

                if (timeoutCount == MaxTimeoutCount)
                {
                    alarmMan.DeviceDownTrigger(DeviceId);
                    origRefresh = RefreshInterval;
                    RefreshInterval *= 2;
                }
                else if (timeoutCount == MaxTimeoutCount * 2)
                {
                    RefreshInterval = MaxDownRefresh;
                }
            }
        }

        protected virtual void ThresholdCheck(double value)
        {
            if (checkTresh)
            {
                if (treshActive)
                {
                    if (trDown < value && !treshOver)
                    {
                        alarmMan.TreshSettTrigger(DeviceId, MeasureType, value, false);
                        treshActive = false;
                    }
                    else if (trUp > value && treshOver)
                    {
                        alarmMan.TreshSettTrigger(DeviceId, MeasureType, value, false);
                        treshActive = false;
                    }

                    return;
                }

                if (value < trDown)
                {
                    alarmMan.TreshExcTrigger(DeviceId, MeasureType, value, false);
                    treshActive = true;
                    treshOver = false;
                }
                else if (value > trUp)
                {
                    alarmMan.TreshExcTrigger(DeviceId, MeasureType, value, true);
                    treshActive = true;
                    treshOver = true;
                }
            }
        }
    }
}
