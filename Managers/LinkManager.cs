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

            test1.OidSysName = new ObjectIdentifier("1.3.6.1.2.1.1.5.0");
            test1.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test1.OidSignal = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test1.RefreshSignal = 1000;
            test1.OidSignalQ = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test1.RefreshSignalQ = 1000;
            test1.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test1.RefreshTx = 1000;
            test1.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test1.RefreshRx = 1000;

            test2.OidSysName = new ObjectIdentifier("1.3.6.1.2.1.1.5.0");
            test2.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test2.OidSignal = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test2.RefreshSignal = 1000;
            test2.OidSignalQ = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test2.RefreshSignalQ = 1000;
            test2.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test2.RefreshTx = 1000;
            test2.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test2.RefreshRx = 1000;

            if (deviceId == 1)
                return test1;
            else
                return test2;
        }
    }
}
