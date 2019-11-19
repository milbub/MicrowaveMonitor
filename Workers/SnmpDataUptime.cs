using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    class SnmpDataUptime : SnmpDataCollector
    {
        public SnmpDataUptime(Device device) : base(device)
        {
            _collectedOid = _device.OidUptime;
            _refreshInterval = Device.UptimeRefresh;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            string uptime = result.First().Data.ToString();
            uptime = uptime.Remove(uptime.Length - 8);
            _device.DataUptime = uptime;
        }
    }
}
