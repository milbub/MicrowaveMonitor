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
        public SnmpTx(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        { }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.DataTx = new Record<uint>(resultTime, UInt32.Parse(result.First().Data.ToString()));
        }
    }
}