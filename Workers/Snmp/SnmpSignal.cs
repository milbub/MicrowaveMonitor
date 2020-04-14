using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Vibrant.InfluxDB.Client.Rows;
using MicrowaveMonitor.Managers;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        private readonly Queue<DynamicInfluxRow> database;

        public SnmpSignal(Queue<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown, Measurement measurement) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown, measurement)
        {
            database = dbRows;
        }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = Math.Abs(double.Parse(result.First().Data.ToString()));
            TresholdCheck(resval);
            Display.DataSig = new Record<double>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);
        }
    }
}