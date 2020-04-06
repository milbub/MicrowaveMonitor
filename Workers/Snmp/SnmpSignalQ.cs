using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
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
        private readonly int divisor;
        private readonly Queue<DynamicInfluxRow> database;

        public SnmpSignalQ(Queue<DynamicInfluxRow> dbRows, int divisor, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        {
            this.divisor = divisor;
            database = dbRows;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = Math.Abs(double.Parse(result.First().Data.ToString()) / divisor);
            Display.DataSigQ = new Record<double>(resultTime, resval); ;
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);

            Diff();
        }

        public override void RecordAvg(double avg)
        {
            Display.AvgSigQ = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (Display.AvgSigQ > 0)
                Display.DiffSigQ = sum / count - Display.AvgSigQ;
        }
    }
}