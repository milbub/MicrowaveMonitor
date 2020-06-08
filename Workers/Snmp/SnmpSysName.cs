using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSysName : SnmpCollector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.All;

        public SnmpSysName(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        { }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            Display.SysName = result.First().Data.ToString();
        }
    }
}