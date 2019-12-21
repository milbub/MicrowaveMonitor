using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpUptime : SnmpCollector
    {
        public SnmpUptime(Device device) : base(device)
        {
            _collectedOid = Device.OidUptime;
            _refreshInterval = Device.RefreshUptime;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            string uptime = result.First().Data.ToString();
            uptime = uptime.Remove(uptime.Length - 8);
            _device.DataUptime = uptime;
        }
    }
}
