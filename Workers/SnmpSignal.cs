﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    public class SnmpSignal : SnmpCollector
    {
        ObservableCollection<UIntRecord> _collectedData;

        public SnmpSignal(Device device) : base(device)
        {
            _collectedOid = Device.OidSignal;
            _refreshInterval = Device.RefreshSignal;
            _collectedData = Device.DataSignal;
        }

        public override void Record(IList<Variable> result, DateTime resultTime)
        {
            _collectedData.Add(new UIntRecord(resultTime, (uint)(Math.Abs(int.Parse(result.First().Data.ToString())))));
        }
    }
}