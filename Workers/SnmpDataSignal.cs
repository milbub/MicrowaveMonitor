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
    class SnmpDataSignal : SnmpDataCollector
    {
        ObservableCollection<UIntRecord> _collectedData;

        public SnmpDataSignal(Device device) : base(device)
        {
            _collectedOid = Device.OidSignalLevel;
            _refreshInterval = Device.SignalLevelRefresh;
            _collectedData = Device.DataSignalLevel;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new UIntRecord(resultTime, UInt32.Parse(result.First().Data.ToString())));
        }
    }
}