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
        private SQLiteConnection LinkDatabase { get; set; } = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, ConfigurationManager.ConnectionStrings["DeviceData"].ConnectionString));
        public Dictionary<int, string> LinkNames { get; private set; } = new Dictionary<int, string>();

        public LinkManager()
        {
            LinkDatabase.CreateTable<Device>();
            LinkDatabase.CreateTable<Link>();

            foreach (Link link in LinkDatabase.Table<Link>())
                LinkNames.Add(link.Id, link.Name);

            Console.WriteLine("0Devices loaded.");
        }

        public TableQuery<Device> GetDeviceTable()
        {
            return LinkDatabase.Table<Device>();
        }

        public Device GetDevice(int id)
        {
            return LinkDatabase.Get<Device>(id);
        }

        public void UpdateDevice(Device device)
        {
            LinkDatabase.Update(device);
        }

        public void AddDevice(Device device)
        {
            LinkDatabase.Insert(device);
        }

        public void DeleteDevice(Device device)
        {
            LinkDatabase.Delete(device);
        }

        public Link GetLink(int id)
        {
            return LinkDatabase.Get<Link>(id);
        }

        public void UpdateLink(Link link)
        {
            LinkDatabase.Update(link);
        }

        public void AddLink(Link link)
        {
            LinkDatabase.Insert(link);
            LinkNames.Add(link.Id, link.Name);
        }

        public void DeleteLink(Link link)
        {
            LinkNames.Remove(link.Id);
            LinkDatabase.Delete(link);
        }
    }
}
