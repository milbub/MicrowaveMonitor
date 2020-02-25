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
        public enum DeviceType { Base, End, R1, R2, R3, R4 };
        public SQLiteConnection LinkDatabase = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, ConfigurationManager.ConnectionStrings["DeviceData"].ConnectionString));
        public Dictionary<int, string> LinkNames = new Dictionary<int, string>();

        public LinkManager()
        {
            LinkDatabase.CreateTable<Device>();
            LinkDatabase.CreateTable<Link>();

            foreach (Link link in LinkDatabase.Table<Link>())
                LinkNames.Add(link.Id, link.Name);
        }

        public Device GetDevice(Link link, DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Base:
                    return LinkDatabase.Get<Device>(link.DeviceBaseId);
                case DeviceType.End:
                    return LinkDatabase.Get<Device>(link.DeviceEndId);
                case DeviceType.R1:
                    return LinkDatabase.Get<Device>(link.DeviceR1Id);
                case DeviceType.R2:
                    return LinkDatabase.Get<Device>(link.DeviceR2Id);
                case DeviceType.R3:
                    return LinkDatabase.Get<Device>(link.DeviceR3Id);
                case DeviceType.R4:
                    return LinkDatabase.Get<Device>(link.DeviceR4Id);
                default:
                    throw new NotSupportedException();
            }
        }

        //// TEMP DEBUG
        //public void InsertDevices()
        //{
        //    Device device1 = new Device()
        //    {
        //        IpAddress = "10.248.16.64",
        //        IsEnabledTx = true,
        //        IsEnabledRx = true,
        //        OidSignal_s = "1.3.6.1.4.1.23688.1.1.5.0",
        //        OidSignalQ_s = "1.3.6.1.4.1.23688.1.1.6.0",
        //        OidTxDataRate_s = "1.3.6.1.4.1.23688.1.1.14.0",
        //        OidRxDataRate_s = "1.3.6.1.4.1.23688.1.1.15.0"
        //    };
        //    Device device2 = new Device()
        //    {
        //        IpAddress = "10.248.16.65",
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
        
        //public void LoadLinks()
        //{
        //    /* TODO: Load from SQL DB */

        //    Device baseDevice = LoadDevice(1);
        //    Device endDevice = LoadDevice(2);
        //    Link testLinka1 = new Link("TEST Summit QAM - Ostrava", 1, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka1.Name, testLinka1);

        //    baseDevice = LoadDevice(3);
        //    endDevice = LoadDevice(4);
        //    Link testLinka2 = new Link("TEST Summit UNI - Praha", 2, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka2.Name, testLinka2);

        //    baseDevice = LoadDevice(5);
        //    endDevice = LoadDevice(6);
        //    Link testLinka3 = new Link("TEST Summit Narrow", 3, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka3.Name, testLinka3);

        //    baseDevice = LoadDevice(7);
        //    endDevice = LoadDevice(8);
        //    Link testLinka4 = new Link("TEST Summit SDV17G", 4, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka4.Name, testLinka4);

        //    baseDevice = LoadDevice(9);
        //    endDevice = LoadDevice(10);
        //    Link testLinka5 = new Link("TEST Summit BTN10", 5, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka5.Name, testLinka5);

        //    baseDevice = LoadDevice(11);
        //    endDevice = LoadDevice(12);
        //    Link testLinka6 = new Link("TEST Summit BT10G", 6, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka6.Name, testLinka6);

        //    baseDevice = LoadDevice(13);
        //    endDevice = LoadDevice(14);
        //    Link testLinka7 = new Link("TEST Summit BTD10", 7, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka7.Name, testLinka7);

        //    baseDevice = LoadDevice(15);
        //    endDevice = LoadDevice(16);
        //    Link testLinka8 = new Link("TEST Ceragon IP-10", 8, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka8.Name, testLinka8);

        //    baseDevice = LoadDevice(17);
        //    endDevice = LoadDevice(18);
        //    Link testLinka9 = new Link("TEST Ceragon IP-20", 9, baseDevice, endDevice);
        //    LinkDatabase.Add(testLinka9.Name, testLinka9);
        //}

        //private Device LoadDevice(int deviceId)
        //{
        //    /* TODO: Load from SQL DB */

        //    // Summit
        //    Device test01 = new Device(1, "10.248.16.64", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test02 = new Device(2, "10.248.16.65", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test03 = new Device(1, "10.248.17.3", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test04 = new Device(2, "10.248.17.4", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test05 = new Device(1, "10.248.37.32", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test06 = new Device(2, "10.248.37.33", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test07 = new Device(1, "10.248.7.14", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);
        //    Device test08 = new Device(2, "10.248.7.15", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0"), 1000, 1000);

        //    // Summit BT
        //    Device test09 = new Device(1, "10.248.12.40", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, 1000);
        //    Device test10 = new Device(2, "10.248.12.41", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, 1000);
        //    Device test11 = new Device(1, "10.248.25.28", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, 1000);
        //    Device test12 = new Device(2, "10.248.25.29", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0"), 1000, new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0"), 1000, 1000);

        //    // Ceragon
        //    Device test13 = new Device(1, "10.248.35.76", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.268451969"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.268451969"), 1000, 1000);
        //    test13.SignalQDivider = 100;
        //    Device test14 = new Device(2, "10.248.35.77", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.268451969"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.268451969"), 1000, 1000);
        //    test14.SignalQDivider = 100;
        //    Device test15 = new Device(1, "10.248.15.186", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.1"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.1"), 1000, 1000);
        //    test15.SignalQDivider = 100;
        //    Device test16 = new Device(2, "10.248.15.187", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.1"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.1"), 1000, 1000);
        //    test16.SignalQDivider = 100;
        //    Device test17 = new Device(1, "10.248.26.32", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.268451969"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.268451969"), 1000, 1000);
        //    test17.SignalQDivider = 100;
        //    Device test18 = new Device(2, "10.248.26.33", 161, "public", new ObjectIdentifier("1.3.6.1.4.1.2281.10.5.1.1.2.268451969"), 1000, new ObjectIdentifier("1.3.6.1.4.1.2281.10.7.1.1.2.268451969"), 1000, 1000);
        //    test18.SignalQDivider = 100;

        //    switch (deviceId)
        //    {
        //        case 1:
        //            return test01;
        //        case 2:
        //            return test02;
        //        case 3:
        //            return test03;
        //        case 4:
        //            return test04;
        //        case 5:
        //            return test05;
        //        case 6:
        //            return test06;
        //        case 7:
        //            return test07;
        //        case 8:
        //            return test08;
        //        case 9:
        //            return test09;
        //        case 10:
        //            return test10;
        //        case 11:
        //            return test11;
        //        case 12:
        //            return test12;
        //        case 13:
        //            return test13;
        //        case 14:
        //            return test14;
        //        case 15:
        //            return test15;
        //        case 16:
        //            return test16;
        //        case 17:
        //            return test17;
        //        case 18:
        //            return test18;
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}
    }
}
