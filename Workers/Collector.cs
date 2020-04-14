using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;

namespace MicrowaveMonitor.Workers
{
    public abstract class Collector
    {
        public static int MaxTimeoutCount { get; set; } = 6;
        public static int MaxTimeout { get; private set; } = 5000;   // ms
        public int RefreshInterval { get; protected set; }

        public int DeviceId { get; protected set; }
        public IPAddress Address { get; protected set; }
        public DeviceDisplay Display { get; protected set; }

        public bool IsRunning { get; protected set; } = false;

        protected Thread tCollector;

        private readonly AlarmManager alarmMan;
        protected readonly Measurement measureType;
        
        private readonly bool checkTresh;
        private readonly float trUp;
        private readonly float trDown;
        private bool treshActive = false;
        private bool treshOver = false;

        private int timeoutCount = 0;

        public Collector(string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown, Measurement measurement)
        {
            Address = IPAddress.Parse(address);
            DeviceId = deviceId;
            RefreshInterval = refreshInterval;
            Display = display;
            alarmMan = alarmManager;
            checkTresh = checkTresholds;
            trUp = treshUp;
            trDown = treshDown;
            measureType = measurement;
        }

        public abstract void Start();

        public void Stop()
        {
            IsRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }

        protected void HasResponded(bool responded)
        {
            /*
            if (responded)
            {
                if (timeoutCount < MaxTimeoutCount)
                    timeoutCount = 0;
                else
                {
                    alarmMan.DeviceUpTrigger(DeviceId);
                    RefreshInterval /= 2;
                    timeoutCount = 0;
                }
            }
            else
            {
                timeoutCount++;

                if (timeoutCount == MaxTimeoutCount)
                {
                    alarmMan.DeviceDownTrigger(DeviceId);
                    RefreshInterval *= 2;
                }
            }*/

            if (responded && timeoutCount <= MaxTimeoutCount)
            {
                timeoutCount = 0;
                return;
            }

            if (responded && timeoutCount > MaxTimeoutCount)
            {
                alarmMan.DeviceUpTrigger(DeviceId);
                timeoutCount = 0;
                return;
            }

            if (!responded && timeoutCount < MaxTimeoutCount)
            {
                timeoutCount++;
                return;
            }

            if (!responded && timeoutCount == MaxTimeoutCount)
            {
                alarmMan.DeviceDownTrigger(DeviceId);
                timeoutCount++;
            }
        }

        protected void TresholdCheck(double value)
        {
            if (checkTresh)
            {
                if (treshActive)
                {
                    if (trDown < value && !treshOver)
                    {
                        alarmMan.TreshSettTrigger(DeviceId, measureType, value);
                        treshActive = false;
                    } else if (trUp > value && treshOver)
                    {
                        alarmMan.TreshSettTrigger(DeviceId, measureType, value);
                        treshActive = false;
                    }
                    
                    return;
                }

                if (value < trDown)
                {
                    alarmMan.TreshExcTrigger(DeviceId, measureType, value, false);
                    treshActive = true;
                    treshOver = false;
                }
                else if (value > trUp)
                {
                    alarmMan.TreshExcTrigger(DeviceId, measureType, value, true);
                    treshActive = true;
                    treshOver = true;
                }
            }
        }
    }
}
