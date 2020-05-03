using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public class PingCollector : Collector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.Latency;

        private readonly Queue<DynamicInfluxRow> database;

        public PingCollector(Queue<DynamicInfluxRow> dbRows, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown) : base(address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        {
            database = dbRows;
        }

        public override void Start()
        {
            if (IsRunning == false)
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

                IsRunning = true;

                tCollector = new Thread(() =>
                {
                    while (IsRunning)
                    {
                        beginTime = DateTime.Now;
                        PingReply reply = pingSender.Send(Address, 1000);
                        finishTime = DateTime.Now;

                        if (reply.Status == IPStatus.Success)
                        {
                            RecordData(reply);
                            HasResponded(true);
                        }
                        else if (reply.Status == IPStatus.TimedOut)
                        {
                            HasResponded(false);
                        }

                        diffTime = finishTime - beginTime;
                        if (diffTime.TotalMilliseconds < RefreshInterval)
                            Thread.Sleep((int)(RefreshInterval - diffTime.TotalMilliseconds));
                    }
                }){ IsBackground = true, Name = "Ping_" + MeasureType + "_device:" + DeviceId };
                tCollector.Start();
            }
        }

        protected virtual void RecordData(PingReply result)
        {
            Display.DataPing = new Record<double>(DateTime.Now, result.RoundtripTime);
            TresholdCheck(result.RoundtripTime);
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = DateTime.Now.ToUniversalTime();
            row.Fields.Add("value", result.RoundtripTime);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);
        }
    }
}
