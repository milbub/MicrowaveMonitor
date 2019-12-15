using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignalQ : SnmpCollector
    {
        ObservableCollection<DoubleRecord> _collectedData;

        public SnmpSignalQ(Device device) : base(device)
        {
            _collectedOid = Device.OidSignalQ;
            _refreshInterval = Device.RefreshSignalQ;
            _collectedData = Device.DataSignalQ;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new DoubleRecord(resultTime, (double)UInt32.Parse(result.First().Data.ToString()) / 10));
        }
    }
}