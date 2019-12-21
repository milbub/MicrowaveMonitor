using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpRx : SnmpCollector
    {
        private ObservableCollection<UIntRecord> _collectedData;

        public SnmpRx(Device device) : base(device)
        {
            _collectedOid = Device.OidRxDataRate;
            _refreshInterval = Device.RefreshRx;
            _collectedData = Device.DataRx;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new UIntRecord(resultTime, UInt32.Parse(result.First().Data.ToString())));
        }
    }
}