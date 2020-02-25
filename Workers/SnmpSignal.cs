using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        public SnmpSignal(Device device) : base(device)
        {
            _collectedOid = Device.OidSignal;
            _refreshInterval = Device.RefreshSignal;
            _collectedData = Device.DataSignal;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new RecordDouble(resultTime, Math.Abs(double.Parse(result.First().Data.ToString()))));
            Diff();
        }

        public override void RecordAvg(double avg)
        {
            _device.AvgSig = avg;
        }

        public override void RecordDiff(double sum, int count)
        {
            if (_device.AvgSig > 0)
                _device.DiffSig = sum / count - _device.AvgSig;
        }
    }
}