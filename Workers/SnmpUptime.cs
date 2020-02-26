﻿using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpUptime : SnmpCollector
    {
        public SnmpUptime(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        { }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
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

            Display.Uptime = uptime;
        }
    }
}
