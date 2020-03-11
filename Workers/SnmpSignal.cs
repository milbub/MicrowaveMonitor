using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        List<DynamicInfluxRow> database;

        public SnmpSignal(List<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        {
            database = dbRows;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = Math.Abs(double.Parse(result.First().Data.ToString()));
            Display.DataSig = new Record<double>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());
            try
            {
                database.Add(row);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(500);
                database.Add(row);
            }
            Diff();
        }

        public override void RecordAvg(double avg)
        {
            Display.AvgSig = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (Display.AvgSig > 0)
                Display.DiffSig = sum / count - Display.AvgSig;
        }
    }
}