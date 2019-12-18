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
            Link testLinka = new Link("TEST Summit - Ostrava", 1, baseDevice, endDevice);

            baseDevice = LoadDevice(3);
            endDevice = LoadDevice(4);
            Link testLinka2 = new Link("TEST Summit - Praha", 2, baseDevice, endDevice);

            LinkDatabase.Add(testLinka.Name, testLinka);
            LinkDatabase.Add(testLinka2.Name, testLinka2);
        }

        private Device LoadDevice(int deviceId)
        {
            /* TODO: Load from SQL DB */

            Device test1 = new Device(1, "10.248.16.64", 161, "public");
            Device test2 = new Device(2, "10.248.16.65", 161, "public");
            Device test3 = new Device(1, "10.248.17.3", 161, "public");
            Device test4 = new Device(2, "10.248.17.4", 161, "public");

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
            test1.RefreshPing = 1000;

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
            test2.RefreshPing = 1000;

            test3.OidSysName = new ObjectIdentifier("1.3.6.1.2.1.1.5.0");
            test3.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test3.OidSignal = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test3.RefreshSignal = 1000;
            test3.OidSignalQ = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test3.RefreshSignalQ = 1000;
            test3.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test3.RefreshTx = 1000;
            test3.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test3.RefreshRx = 1000;
            test3.RefreshPing = 1000;

            test4.OidSysName = new ObjectIdentifier("1.3.6.1.2.1.1.5.0");
            test4.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test4.OidSignal = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test4.RefreshSignal = 1000;
            test4.OidSignalQ = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test4.RefreshSignalQ = 1000;
            test4.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test4.RefreshTx = 1000;
            test4.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test4.RefreshRx = 1000;
            test4.RefreshPing = 1000;

            if (deviceId == 1)
                return test1;
            else if (deviceId == 2)
                return test2;
            else if (deviceId == 3)
                return test3;
            else
                return test4;
        }
    }
}
