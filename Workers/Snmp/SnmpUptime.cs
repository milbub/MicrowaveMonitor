using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicrowaveMonitor.Workers
{
    public class SnmpUptime : SnmpCollector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.All;

        public SnmpUptime(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        { }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
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
