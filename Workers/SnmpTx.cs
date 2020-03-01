﻿using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public class SnmpTx : SnmpCollector
    {
        List<DynamicInfluxRow> database;

        public SnmpTx(List<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(oid, port, community, address, deviceId, refreshInterval, display)
        {
            database = dbRows;
        }

        public override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            uint resval = UInt32.Parse(result.First().Data.ToString());
            Display.DataTx = new Record<uint>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow();
            row.Timestamp = resultTime.ToUniversalTime();
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());
            database.Add(row);
            Diff();
        }
    }
}