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
        public SnmpSignalQ(Device device) : base(device)
        {
            _collectedOid = Device.OidSignalQ;
            _refreshInterval = Device.RefreshSignalQ;
            _collectedData = Device.DataSignalQ;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new RecordDouble(resultTime, Math.Abs(double.Parse(result.First().Data.ToString()) / Device.SignalQDivider)));
            Diff();
        }

        public override void RecordAvg(double avg)
        {
            _device.AvgSigQ = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (_device.AvgSigQ > 0)
                _device.DiffSigQ = sum / count - _device.AvgSigQ;
        }
    }
}