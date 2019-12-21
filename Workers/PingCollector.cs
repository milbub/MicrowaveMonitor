using MicrowaveMonitor.Database;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Workers
{
    public class PingCollector : Collector
    {
        private Thread tCollector;

        public PingCollector(Device device) : base(device) { }

        public override void Start()
        {
            DateTime beginTime;
            DateTime finishTime;
            TimeSpan diffTime;
            Ping pingSender = new Ping();

            int timeout;
            if (Device.RefreshPing > MaxTimeout)
                timeout = MaxTimeout;
            else
                timeout = Device.RefreshPing * 2;

            _isRunning = true;

            tCollector = new Thread(() =>
            {
                while (_isRunning)
                {
                    beginTime = DateTime.Now;
                    PingReply reply = pingSender.Send(Device.Address.Address, 1000);
                    finishTime = DateTime.Now;

                    if (reply.Status == IPStatus.Success)
                        Record(reply);

                    diffTime = finishTime - beginTime;
                    if (diffTime.TotalMilliseconds < Device.RefreshPing)
                        Thread.Sleep((int)(Device.RefreshPing - diffTime.TotalMilliseconds));
                }
            });
            tCollector.Start();
        }

        public virtual void Record(PingReply result)
        {
            Device.DataPing.Add(new DoubleRecord(DateTime.Now, result.RoundtripTime));
        }

        public override void Stop()
        {
            _isRunning = false;
            if (Device.RefreshPing > MaxTimeout)
                tCollector.Abort();
        }

        private const int avgAge = 60;
        private const int diffAge = 30;

        public void StartStatistic()
        {
            Task.Run(() =>
            {
                while (IsRunning)
                {
                    Thread.Sleep(90000);
                    avg();
                }
            });
        }

        public void avg()
        {
            TimeSpan timediff = new TimeSpan(avgAge * 10000000);
            DateTime old = DateTime.Now - timediff;
            double sum = 0;
            int i;

            for (i = 0; i < _device.DataPing.Count; i++)
            {
                if (_device.DataPing[i].TimeMark < old)
                    sum += _device.DataPing[i].Data;
                else
                    break;
            }

            _device.AvgPing = sum / i;
        }

        public void diff()
        {
            TimeSpan timediff = new TimeSpan(diffAge * 10000000);
            DateTime old = DateTime.Now - timediff;
            double sum = 0;
            int x = 0;
            int i;

            for (i = (_device.DataPing.Count - 1); i >= 0; i--)
            {
                if (_device.DataPing[i].TimeMark > old)
                {
                    sum += _device.DataPing[i].Data;
                    x += 1;
                }
                else
                    break;
            }

            if (_device.AvgPing > 0)
                _device.DiffPing = sum / x - _device.AvgPing;
        }
    }
}
