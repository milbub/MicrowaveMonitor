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
    public class SnmpSysName : SnmpCollector
    {
        public SnmpSysName(Device device) : base(device)
        {
            _collectedOid = Device.OidSysName;
            _refreshInterval = Device.RefreshSysName;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _device.DataSysName = result.First().Data.ToString(); ;
        }
    }
}