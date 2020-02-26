using MicrowaveMonitor.Database;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Workers
{
    public class PingCollector : Collector
    {
        public PingCollector(string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(address, deviceId, refreshInterval, display)
        { }

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
                    PingReply reply = pingSender.Send(Address, 1000);
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
            Display.DataPing = new Record<double>(DateTime.Now, result.RoundtripTime);
            Diff();
        }

        public override void RecordAvg(double avg)
        {
            Display.AvgPing = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (Display.AvgPing > 0)
                Display.DiffPing = sum / count - Display.AvgPing;
        }
    }
}
