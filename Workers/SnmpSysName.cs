using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSysName : SnmpCollector
    {
        public SnmpSysName(Device device) : base(device)
        {
            _collectedOid = Device.OidSysName;
            _refreshInterval = Device.RefreshSysName;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            _device.DataSysName = result.First().Data.ToString();
        }
    }
}