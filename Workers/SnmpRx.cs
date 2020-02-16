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
        protected ObservableCollection<RecordUInt> _collectedDataUInt;
        public ObservableCollection<RecordUInt> CollectedDataUInt { get => _collectedDataUInt; }

        public SnmpRx(Device device) : base(device)
        {
            _collectedOid = Device.OidRxDataRate;
            _refreshInterval = Device.RefreshRx;
            _collectedDataUInt = Device.DataRx;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            _collectedDataUInt.Add(new RecordUInt(resultTime, UInt32.Parse(result.First().Data.ToString())));
        }
    }
}