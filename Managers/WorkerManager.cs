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
        public Dictionary<int, DeviceDisplay> DeviceToFront { get; set; } = new Dictionary<int, DeviceDisplay>();

        private List<Collector> workers = new List<Collector>();
        private WeatherCollector weatherCollector;

        public void InitWorkers(TableQuery<Device> devices, DataManager dataDb)
        {
            weatherCollector = new WeatherCollector(dataDb.WeatherTempTransactions, DeviceToFront);

            foreach (Device device in devices)
            {
                DeviceToFront.Add(device.Id, new DeviceDisplay());

                workers.Add(new PingCollector(dataDb.PingTransactions, device.Address, device.Id, device.RefreshPing, DeviceToFront[device.Id]));
                workers.Add(new SnmpSysName(Device.OidSysName_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshSysName, DeviceToFront[device.Id]));
                workers.Add(new SnmpUptime(Device.OidUptime_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshUptime, DeviceToFront[device.Id]));
                if (device.IsEnabledSignal)
                    workers.Add(new SnmpSignal(dataDb.SignalTransactions, device.OidSignal_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignal, DeviceToFront[device.Id]));
                if (device.IsEnabledSignalQ)
                    workers.Add(new SnmpSignalQ(dataDb.SignalQTransactions, device.SignalQDivisor, device.OidSignalQ_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignalQ, DeviceToFront[device.Id]));
                if (device.IsEnabledTx)
                    workers.Add(new SnmpTx(dataDb.TxTransactions, device.OidTxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTx, DeviceToFront[device.Id]));
                if (device.IsEnabledRx)
                    workers.Add(new SnmpRx(dataDb.RxTransactions, device.OidRxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshRx, DeviceToFront[device.Id]));
                if (device.IsEnabledTempOdu)
                    workers.Add(new SnmpTempOdu(dataDb.TempOduTransactions, device.OidTempOdu_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTempOdu, DeviceToFront[device.Id]));
                if (device.IsEnabledTempIdu)
                    workers.Add(new SnmpTempIdu(dataDb.TempIduTransactions, device.OidTempIdu_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTempIdu, DeviceToFront[device.Id]));
                if (device.IsEnabledVoltage)
                    workers.Add(new SnmpVoltage(dataDb.VoltageTransactions, device.OidVoltage_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshVoltage, DeviceToFront[device.Id]));

                weatherCollector.AddDevice(device.Id, device.Latitude, device.Longitude);
            }

            StartWorkers();
        }

        public void StartWorkers()
        {
            foreach (Collector worker in workers)
            {
                worker.Start();
            }
            weatherCollector.Start();
        }

        public void StopWorkers()
        {
            foreach (Collector worker in workers)
            {
                worker.Stop();
            }
            weatherCollector.Stop();
        }
    }
}