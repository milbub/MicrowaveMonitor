using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignalQ : SnmpCollector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.Quality;

        private readonly int divisor;
        private readonly Queue<DynamicInfluxRow> database;

        public SnmpSignalQ(Queue<DynamicInfluxRow> dbRows, int divisor, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        {
            this.divisor = divisor;
            database = dbRows;
        }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = Math.Abs(double.Parse(result.First().Data.ToString()) / divisor);
            TresholdCheck(resval);
            Display.DataSigQ = new Record<double>(resultTime, resval); ;
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);
        }
    }
}