using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignalQ : SnmpCollector
    {
        int divisor;

        public SnmpSignalQ(int divisor, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        {
            this.divisor = divisor;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.DataSigQ = new Record<double>(resultTime, Math.Abs(double.Parse(result.First().Data.ToString()) / divisor));
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