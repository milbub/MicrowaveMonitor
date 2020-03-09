using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    class SnmpTempIdu : SnmpCollector
    {
        List<DynamicInfluxRow> database;

        public SnmpTempIdu(List<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        {
            database = dbRows;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = double.Parse(result.First().Data.ToString());
            Display.DataTempIdu = new Record<double>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());
            database.Add(row);
        }
    }
}
