using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using SQLite;
using System.IO;

namespace MicrowaveMonitor.Managers
{
    public class LinkManager
    {
        private SQLiteConnection LinkDatabase { get; set; } = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, ConfigurationManager.ConnectionStrings["DeviceData"].ConnectionString));
        private SQLiteConnection AlarmDatabase { get; set; } = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, ConfigurationManager.ConnectionStrings["AlarmData"].ConnectionString));

        public Dictionary<int, string> LinkNames { get; private set; } = new Dictionary<int, string>();

        public LinkManager()
        {
            LinkDatabase.CreateTable<Device>();
            LinkDatabase.CreateTable<Link>();
            AlarmDatabase.CreateTable<Alarm>();

            lock (LinkDatabase)
                foreach (Link link in LinkDatabase.Table<Link>())
                    LinkNames.Add(link.Id, link.Name);

            Console.WriteLine("0Device definitions loaded.");
        }

        public TableQuery<Device> GetDeviceTable()
        {
            lock (LinkDatabase)
                return LinkDatabase.Table<Device>();
        }

        public Device GetDevice(int id)
        {
            lock (LinkDatabase)
                return LinkDatabase.Get<Device>(id);
        }

        public void UpdateDevice(Device device)
        {
            lock (LinkDatabase)
                LinkDatabase.Update(device);
        }

        public void AddDevice(Device device)
        {
            lock (LinkDatabase)
                LinkDatabase.Insert(device);
        }

        public void DeleteDevice(Device device)
        {
            lock (LinkDatabase)
                LinkDatabase.Delete(device);
        }

        public string GetDeviceType(int deviceId)
        {
            TableQuery<Link> query;

            lock (LinkDatabase)
            {
                query = LinkDatabase.Table<Link>().Where(v => (v.DeviceBaseId == deviceId)
                || (v.DeviceEndId == deviceId) || (v.DeviceR1Id == deviceId)
                || (v.DeviceR2Id == deviceId) || (v.DeviceR3Id == deviceId)
                || (v.DeviceR4Id == deviceId));
            }

            if (query.First().DeviceBaseId == deviceId)
                return "A";
            else if (query.First().DeviceEndId == deviceId)
                return "B";
            else if (query.First().DeviceR1Id == deviceId)
                return "R1";
            else if (query.First().DeviceR2Id == deviceId)
                return "R2";
            else if (query.First().DeviceR3Id == deviceId)
                return "R3";
            else if (query.First().DeviceR4Id == deviceId)
                return "R4";

            return null;
        }

        public Link GetLink(int id)
        {
            lock (LinkDatabase)
                return LinkDatabase.Get<Link>(id);
        }

        public void UpdateLink(Link link)
        {
            lock (LinkDatabase)
                LinkDatabase.Update(link);
        }

        public void AddLink(Link link)
        {
            lock (LinkDatabase)
                LinkDatabase.Insert(link);
            lock (LinkNames)
                LinkNames.Add(link.Id, link.Name);
        }

        public void DeleteLink(Link link)
        {
            lock (LinkNames)
                LinkNames.Remove(link.Id);
            lock (LinkDatabase)
                LinkDatabase.Delete(link);
        }

        public int FindLinkByDevice(int deviceId)
        {
            TableQuery<Link> query;

            lock (LinkDatabase)
            {
                query = LinkDatabase.Table<Link>().Where(v => (v.DeviceBaseId == deviceId)
                || (v.DeviceEndId == deviceId) || (v.DeviceR1Id == deviceId)
                || (v.DeviceR2Id == deviceId) || (v.DeviceR3Id == deviceId)
                || (v.DeviceR4Id == deviceId));
            }

            return query.First().Id;
        }

        public Alarm GetAlarm(int id)
        {
            lock (AlarmDatabase)
                return AlarmDatabase.Get<Alarm>(id);
        }

        public TableQuery<Alarm> GetListedAlarmsTable()
        {
            TableQuery<Alarm> query;

            lock (AlarmDatabase)
            {
                query = AlarmDatabase.Table<Alarm>().Where(v => v.IsActive || v.IsShowed);
            }

            return query;
        }

        public TableQuery<Alarm> GetDeviceAlarmsTable(int devId, int limit)
        {
            TableQuery<Alarm> query;

            lock (AlarmDatabase)
            {
                query = AlarmDatabase.Table<Alarm>().Where(v => v.DeviceId == devId).OrderByDescending(x => x.GenerTime).Take(limit);
            }

            return query;
        }

        public void UpdateAlarm(Alarm alarm)
        {
            lock (AlarmDatabase)
                AlarmDatabase.Update(alarm);
        }

        public void AddAlarm(Alarm alarm)
        {
            lock (AlarmDatabase)
                AlarmDatabase.Insert(alarm);
        }
    }
}
