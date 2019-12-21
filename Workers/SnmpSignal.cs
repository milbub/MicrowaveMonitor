﻿using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        private ObservableCollection<DoubleRecord> _collectedData;

        public SnmpSignal(Device device) : base(device)
        {
            _collectedOid = Device.OidSignal;
            _refreshInterval = Device.RefreshSignal;
            _collectedData = Device.DataSignal;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new DoubleRecord(resultTime, Math.Abs(double.Parse(result.First().Data.ToString()))));
            Diff();
        }

        private const int avgAge = 60;
        private const int diffAge = 30;

        public void StartStatistic()
        {
            Task.Run(() =>
            {
                while (IsRunning)
                {
                    Thread.Sleep(90000);
                    Avg();
                }
            });
        }

        public void Avg()
        {
            TimeSpan timediff = new TimeSpan(avgAge * 10000000);
            DateTime old = DateTime.Now - timediff;
            double sum = 0;
            int i;

            for (i = 0; i < _collectedData.Count; i++)
            {
                if (_collectedData[i].TimeMark < old)
                    sum += _collectedData[i].Data;
                else
                    break;
            }

            _device.AvgSig = sum / i;
        }

        public void Diff()
        {
            TimeSpan timediff = new TimeSpan(diffAge * 10000000);
            DateTime old = DateTime.Now - timediff;
            double sum = 0;
            int x = 0;
            int i;

            for (i = (_collectedData.Count - 1); i >= 0; i--)
            {
                if (_collectedData[i].TimeMark > old)
                {
                    sum += _collectedData[i].Data;
                    x += 1;
                }
                else
                    break;
            }

            if (_device.AvgSig > 0)
                _device.DiffSig = sum / x - _device.AvgSig;
        }
    }
}