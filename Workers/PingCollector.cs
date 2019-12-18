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
    public class PingCollector : Collector
    {
        private Thread tCollector;

        public PingCollector(Device device) : base(device){}

        public void Start()
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
    }
}
