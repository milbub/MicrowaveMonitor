﻿using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    class SnmpTempIdu : SnmpCollector
    {
        protected override Measurement MeasureType { get { return measureType; } }
        private static readonly Measurement measureType = Measurement.TempIDU;

        public bool IsOutdoorBound { get; set; }

        private readonly Queue<DynamicInfluxRow> database;

        public SnmpTempIdu(Queue<DynamicInfluxRow> dbRows, string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown, bool isOutdoorBound) : base(oid, port, community, address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown)
        {
            database = dbRows;
            IsOutdoorBound = isOutdoorBound;
        }

        protected override void RecordData(IList<Variable> result, DateTime resultTime)
        {
            double resval = double.Parse(result.First().Data.ToString());
            ThresholdCheck(resval);
            Display.DataTempIdu = new Record<double>(resultTime, resval);
            DynamicInfluxRow row = new DynamicInfluxRow { Timestamp = resultTime.ToUniversalTime() };
            row.Fields.Add("value", resval);
            row.Tags.Add("device", DeviceId.ToString());

            lock (database)
                database.Enqueue(row);
        }

        protected override void ThresholdCheck(double value)
        {
            if (IsOutdoorBound)
            {
                if (checkTresh && Display.WeatherTemp != null)
                {
                    if (treshActive)
                    {
                        if ((Display.WeatherTemp - trDown) < value && !treshOver)
                        {
                            alarmMan.TreshSettTrigger(DeviceId, MeasureType, value, false);
                            treshActive = false;
                        }
                        else if ((Display.WeatherTemp + trUp) > value && treshOver)
                        {
                            alarmMan.TreshSettTrigger(DeviceId, MeasureType, value, false);
                            treshActive = false;
                        }

                        return;
                    }

                    if (value < (Display.WeatherTemp - trDown))
                    {
                        alarmMan.TreshExcTrigger(DeviceId, MeasureType, value, false);
                        treshActive = true;
                        treshOver = false;
                    }
                    else if (value > (Display.WeatherTemp + trUp))
                    {
                        alarmMan.TreshExcTrigger(DeviceId, MeasureType, value, true);
                        treshActive = true;
                        treshOver = true;
                    }
                }
            }
            else
                base.ThresholdCheck(value);
        }
    }
}
