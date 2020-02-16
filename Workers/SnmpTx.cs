using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpTx : SnmpCollector
    {
        protected ObservableCollection<RecordUInt> _collectedDataUInt;
        public ObservableCollection<RecordUInt> CollectedDataUInt { get => _collectedDataUInt; }

        public SnmpTx(Device device) : base(device)
        {
            _collectedOid = Device.OidTxDataRate;
            _refreshInterval = Device.RefreshTx;
            _collectedDataUInt = Device.DataTx;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            _collectedDataUInt.Add(new RecordUInt(resultTime, UInt32.Parse(result.First().Data.ToString())));
        }
    }
}