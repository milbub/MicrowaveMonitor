using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSysName : SnmpCollector
    {
        public SnmpSysName(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        { }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.SysName = result.First().Data.ToString();
        }
    }
}