using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using SQLite;
using System.IO;

namespace MicrowaveMonitor.Managers
{
    public class LinkManager
    {
        public SQLiteConnection LinkDatabase { get; private set; } = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, ConfigurationManager.ConnectionStrings["DeviceData"].ConnectionString));
        public Dictionary<int, string> LinkNames { get; private set; } = new Dictionary<int, string>();

        public LinkManager()
        {
            LinkDatabase.CreateTable<Device>();
            LinkDatabase.CreateTable<Link>();

            foreach (Link link in LinkDatabase.Table<Link>())
                LinkNames.Add(link.Id, link.Name);
        }

        public Device GetDevice(int id)
        {
            return LinkDatabase.Get<Device>(id);
        }

        public void UpdateDevice(Device device)
        {
            LinkDatabase.Update(device);
        }

        //// TEMP DEBUG
        //public void InsertDevices()
        //{
        //    Device device1 = new Device()
        //    {
        //        Address = "10.248.16.64",
        //        IsEnabledTx = true,
        //        IsEnabledRx = true,
        //        OidSignal_s = "1.3.6.1.4.1.23688.1.1.5.0",
        //        OidSignalQ_s = "1.3.6.1.4.1.23688.1.1.6.0",
        //        OidTxDataRate_s = "1.3.6.1.4.1.23688.1.1.14.0",
        //        OidRxDataRate_s = "1.3.6.1.4.1.23688.1.1.15.0"
        //    };
        //    Device device2 = new Device()
        //    {
        //        Address = "10.248.16.65",
        //        IsEnabledTx = true,
        //        IsEnabledRx = true,
        //        OidSignal_s = "1.3.6.1.4.1.23688.1.1.5.0",
        //        OidSignalQ_s = "1.3.6.1.4.1.23688.1.1.6.0",
        //        OidTxDataRate_s = "1.3.6.1.4.1.23688.1.1.14.0",
        //        OidRxDataRate_s = "1.3.6.1.4.1.23688.1.1.15.0"
        //    };
        //    LinkDatabase.Insert(device1);
        //    LinkDatabase.Insert(device2);
        //    InsertLink("Summit QAM - Ostrava", device1.Id, device2.Id);
        //}

        //// TEMP DEBUG
        //public void InsertLink(string name, int devBase, int devEnd)
        //{
        //    Link link = new Link()
        //    {
        //        Name = name,
        //        HopCount = 1,
        //        DeviceBaseId = devBase,
        //        DeviceEndId = devEnd
        //    };
        //    LinkDatabase.Insert(link);
        //}
    }
}
