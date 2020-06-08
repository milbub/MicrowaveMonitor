﻿using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.Strength;

        private readonly Queue<DynamicInfluxRow> database;

        public SnmpSignal(Queue<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        {
            database = dbRows;
        }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = (-1) * Math.Abs(double.Parse(result.First().Data.ToString()));
            ThresholdCheck(resval);
            Display.DataSig = new Record<double>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow { Timestamp = resultTime.ToUniversalTime() };
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);
        }
    }
}