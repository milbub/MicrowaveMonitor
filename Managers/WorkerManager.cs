using MicrowaveMonitor.Database;
using MicrowaveMonitor.Workers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace MicrowaveMonitor.Managers
{
    public class WorkerManager
    {
        private List<Collector> Workers = new List<Collector>();
        public Dictionary<int, DeviceDisplay> DeviceToFront = new Dictionary<int, DeviceDisplay>();

        public void InitWorkers(TableQuery<Device> devices)
        {
            foreach (Device device in devices)
            {
                DeviceToFront.Add(device.Id, new DeviceDisplay());

                Workers.Add(new PingCollector(device.Address, device.Id, device.RefreshPing, DeviceToFront[device.Id]));
                Workers.Add(new SnmpSysName(Device.OidSysName_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshSysName, DeviceToFront[device.Id]));
                Workers.Add(new SnmpUptime(Device.OidUptime_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshUptime, DeviceToFront[device.Id]));
                if (device.IsEnabledSignal)
                    Workers.Add(new SnmpSignal(device.OidSignal_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignal, DeviceToFront[device.Id]));
                if (device.IsEnabledSignalQ)
                    Workers.Add(new SnmpSignalQ(device.SignalQDivisor, device.OidSignalQ_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignalQ, DeviceToFront[device.Id]));
                if (device.IsEnabledTx)
                    Workers.Add(new SnmpTx(device.OidTxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTx, DeviceToFront[device.Id]));
                if (device.IsEnabledRx)
                    Workers.Add(new SnmpRx(device.OidRxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshRx, DeviceToFront[device.Id]));
            }

            StartWorkers();
        }

        public void StartWorkers()
        {
            foreach (Collector worker in Workers)
            {
                worker.Start();
            }
        }

        public void StopWorkers()
        {
            foreach (Collector worker in Workers)
            {
                worker.Stop();
            }
        }
    }
}
