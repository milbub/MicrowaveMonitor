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
        public Dictionary<int, DeviceDisplay> DeviceToFront { get; set; }

        private readonly DataManager data;
        private readonly LinkManager links;
        private readonly AlarmManager alarms;

        private readonly List<Collector> workers = new List<Collector>();
        private WeatherCollector weatherCollector;

        public WorkerManager(DataManager data, LinkManager links, AlarmManager alarms, Dictionary<int, DeviceDisplay> deviceToFront)
        {
            this.data = data;
            this.links = links;
            this.alarms = alarms;
            DeviceToFront = deviceToFront;
        }

        public void InitWorkers(TableQuery<Device> devices)
        {
            weatherCollector = new WeatherCollector(data.WeatherTempTransactions, data.WeatherOtherTransactions, DeviceToFront);

            foreach (Device device in devices)
            {
                InitDevice(device);
            }

            weatherCollector.IsRunning = true;
        }

        public void InitDevice(Device device)
        {
            DeviceToFront.Add(device.Id, new DeviceDisplay() { Id = device.Id });
            
            if (!device.IsPaused)
            {
                StartDevice(device);
            }
            else
                DeviceToFront[device.Id].State = DeviceDisplay.LinkState.Paused;

            alarms.RegisterListener(device);
        }

        public void StartDevice(Device device)
        {
            PingCollector pingCollector = new PingCollector(data.PingTransactions, device.Address, device.Id, device.RefreshPing, DeviceToFront[device.Id], alarms, device.TresholdPing, device.TreshUpPing, device.TreshDownPing);
            pingCollector.Start();
            workers.Add(pingCollector);

            SnmpSysName snmpSysName = new SnmpSysName(Device.OidSysName_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshSysName, DeviceToFront[device.Id], alarms, false, 0, 0);
            snmpSysName.Start();
            workers.Add(snmpSysName);

            SnmpUptime snmpUptime = new SnmpUptime(Device.OidUptime_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, Device.RefreshUptime, DeviceToFront[device.Id], alarms, false, 0, 0);
            snmpUptime.Start();
            workers.Add(snmpUptime);

            if (device.IsEnabledSignal)
            {
                SnmpSignal snmpSignal = new SnmpSignal(data.SignalTransactions, device.OidSignal_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignal, DeviceToFront[device.Id], alarms, device.TresholdSignal, device.TreshUpSignal, device.TreshDownSignal);
                snmpSignal.Start();
                workers.Add(snmpSignal);
            }

            if (device.IsEnabledSignalQ)
            {
                SnmpSignalQ snmpSignalQ = new SnmpSignalQ(data.SignalQTransactions, device.SignalQDivisor, device.OidSignalQ_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshSignalQ, DeviceToFront[device.Id], alarms, device.TresholdSignalQ, device.TreshUpSignalQ, device.TreshDownSignalQ);
                snmpSignalQ.Start();
                workers.Add(snmpSignalQ);
            }

            if (device.IsEnabledTx)
            {
                SnmpTx snmpTx = new SnmpTx(data.TxTransactions, device.OidTxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTx, DeviceToFront[device.Id], alarms, false, 0, 0);
                snmpTx.Start();
                workers.Add(snmpTx);
            }

            if (device.IsEnabledRx)
            {
                SnmpRx snmpRx = new SnmpRx(data.RxTransactions, device.OidRxDataRate_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshRx, DeviceToFront[device.Id], alarms, false, 0, 0);
                snmpRx.Start();
                workers.Add(snmpRx);
            }

            if (device.IsEnabledTempOdu)
            {
                SnmpTempOdu snmpTempOdu = new SnmpTempOdu(data.TempOduTransactions, device.OidTempOdu_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTempOdu, DeviceToFront[device.Id], alarms, device.TresholdTempOdu, device.TreshUpTempOdu, device.TreshDownTempOdu);
                snmpTempOdu.Start();
                workers.Add(snmpTempOdu);
            }

            if (device.IsEnabledTempIdu)
            {
                SnmpTempIdu snmpTempIdu = new SnmpTempIdu(data.TempIduTransactions, device.OidTempIdu_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshTempIdu, DeviceToFront[device.Id], alarms, device.TresholdTempIdu, device.TreshUpTempIdu, device.TreshDownTempIdu, device.IsTempIduOutdoor);
                snmpTempIdu.Start();
                workers.Add(snmpTempIdu);
            }

            if (device.IsEnabledVoltage)
            {
                SnmpVoltage snmpVoltage = new SnmpVoltage(data.VoltageTransactions, device.OidVoltage_s, device.SnmpPort, device.CommunityString, device.Address, device.Id, device.RefreshVoltage, DeviceToFront[device.Id], alarms, device.TresholdVoltage, device.TreshUpVoltage, device.TreshDownVoltage);
                snmpVoltage.Start();
                workers.Add(snmpVoltage);
            }

            if (device.Latitude != "0" && device.Longitude != "0")
                weatherCollector.AddDevice(device.Id, device.Latitude, device.Longitude);

            device.IsPaused = false;
            links.UpdateDevice(device);
            DeviceToFront[device.Id].State = DeviceDisplay.LinkState.Running;
        }

        public void StopDevice(Device device)
        {
            if (!device.IsPaused)
            {
                for (int i = workers.Count - 1; i >= 0; i--)
                {
                    if (workers[i].DeviceId == device.Id)
                    {
                        workers[i].Stop();
                        workers.RemoveAt(i);
                    }
                }

                weatherCollector.RemoveDevice(device.Id);

                device.IsPaused = true;
                links.UpdateDevice(device);
                DeviceToFront[device.Id].State = DeviceDisplay.LinkState.Paused;
                alarms.DeviceStopped(device.Id);
            }
        }

        public void RemoveDevice(Device device)
        {
            StopDevice(device);
            DeviceToFront.Remove(device.Id);
            alarms.UnregisterListener(device);
        }

        public void RestartDevice(Device device)
        {
            alarms.UpdateDeviceWatch(device);
            StopDevice(device);
            StartDevice(device);
        }
    }
}