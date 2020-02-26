using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        public SnmpSignal(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        { }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.DataSig = new Record<double>(resultTime, Math.Abs(double.Parse(result.First().Data.ToString())));
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