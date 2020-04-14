using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSysName : SnmpCollector
    {
        public SnmpSysName(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown, Measurement measurement) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown, measurement)
        { }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.SysName = result.First().Data.ToString();
        }
    }
}