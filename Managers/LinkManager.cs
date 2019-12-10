using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrowaveMonitor.Database;
using Lextm.SharpSnmpLib;

namespace MicrowaveMonitor.Managers
{
    class LinkManager
    {
        Dictionary<string, Link> _linkDatabase = new Dictionary<string, Link>();

        public Dictionary<string, Link> LinkDatabase { get => _linkDatabase; set => _linkDatabase = value; }

        public void LoadLinks()
        {
            /* TODO: Load from SQL DB */

            Device baseDevice = LoadDevice(1);
            Device endDevice = LoadDevice(2);
            Link testLinka = new Link("TEST Summit QAM - 10.248.16.64 <-> 10.248.16.65", 1, baseDevice, endDevice);

            LinkDatabase.Add(testLinka.Name, testLinka);
        }

        private Device LoadDevice(int deviceId)
        {
            /* TODO: Load from SQL DB */

            Device test1 = new Device(1, "10.248.16.64", 161, "public");
            Device test2 = new Device(2, "10.248.16.65", 161, "public");

            test1.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test1.OidSignalLevel = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test1.SignalLevelRefresh = 1000;
            test1.OidSignalQuality = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test1.SignalQualityRefresh = 1000;
            test1.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test1.TxRefresh = 1000;
            test1.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test1.RxRefresh = 1000;

            test2.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test2.OidSignalLevel = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test2.SignalLevelRefresh = 1000;
            test2.OidSignalQuality = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test2.SignalQualityRefresh = 1000;
            test2.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test2.TxRefresh = 1000;
            test2.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test2.RxRefresh = 1000;

            if (deviceId == 1)
                return test1;
            else
                return test2;
        }
    }
}
