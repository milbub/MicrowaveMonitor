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
            uint uptime = 0;

            if (result.First().Data.GetType() == typeof(TimeTicks))
            {
                TimeTicks uptimeTicks = (TimeTicks)result.First().Data;
                uptime = uptimeTicks.ToUInt32();
            }
            else if (result.First().Data.GetType() == typeof(Integer32))
            {
                Integer32 uptimeTicks = (Integer32)result.First().Data;
                uptime = (uint)uptimeTicks.ToInt32();
            }

            _device.DataUptime = uptime;
        }
    }
}
