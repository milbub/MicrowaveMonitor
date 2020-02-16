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

        public PingCollector(Device device) : base(device)
        {
            _refreshInterval = Device.RefreshPing;
            _collectedData = Device.DataPing;
        }

        public override void Start()
        {
            DateTime beginTime;
            DateTime finishTime;
            TimeSpan diffTime;
            Ping pingSender = new Ping();

            int timeout;
            if (RefreshInterval > MaxTimeout)
                timeout = MaxTimeout;
            else
                timeout = RefreshInterval * 2;

            _isRunning = true;

            tCollector = new Thread(() =>
            {
                while (_isRunning)
                {
                    beginTime = DateTime.Now;
                    PingReply reply = pingSender.Send(Device.Address.Address, 1000);
                    finishTime = DateTime.Now;

                    if (reply.Status == IPStatus.Success)
                        RecordData(reply);

                    diffTime = finishTime - beginTime;
                    if (diffTime.TotalMilliseconds < RefreshInterval)
                        Thread.Sleep((int)(RefreshInterval - diffTime.TotalMilliseconds));
                }
            });
            tCollector.Start();
        }

        public virtual void RecordData(PingReply result)
        {
            _collectedData.Add(new RecordDouble(DateTime.Now, result.RoundtripTime));
            Diff();
        }

        public override void RecordAvg(double avg)
        {
            _device.AvgPing = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (_device.AvgPing > 0)
                _device.DiffPing = sum / count - _device.AvgPing;
        }

        public override void Stop()
        {
            _isRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }
    }
}
