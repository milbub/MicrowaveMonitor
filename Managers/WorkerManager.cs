using MicrowaveMonitor.Database;
using MicrowaveMonitor.Workers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Managers
{
    public class WorkerManager
    {
        public void InitWorkers(Dictionary<string, Link> linkDatabase)
        {
            Task.Run(() =>
            {
                Random randomizer = new Random();

                foreach (Link link in linkDatabase.Values)
                {
                    switch (link.HopCount)
                    {
                        case 0:
                            InitDeviceWorkers(link.BaseDevice);
                            break;
                        case 1:
                            InitDeviceWorkers(link.EndDevice);
                            goto case 0;
                        case 2:
                            InitDeviceWorkers(link.RelayOne);
                            goto case 1;
                        case 3:
                            InitDeviceWorkers(link.RelayTwo);
                            goto case 2;
                        case 4:
                            InitDeviceWorkers(link.RelayThree);
                            goto case 3;
                        case 5:
                            InitDeviceWorkers(link.RelayFour);
                            goto case 4;
                        default:
                            throw new NotSupportedException();
                    }

                    Thread.Sleep(randomizer.Next(10));
                }
            });
        }

        public void StartWorkers(Dictionary<string, Link> linkDatabase)
        {
            Task.Run(() =>
            {
                Random randomizer = new Random();

                foreach (Link link in linkDatabase.Values)
                {
                    switch (link.HopCount)
                    {
                        case 0:
                            StartDeviceWorkers(link.BaseDevice);
                            break;
                        case 1:
                            StartDeviceWorkers(link.EndDevice);
                            goto case 0;
                        case 2:
                            StartDeviceWorkers(link.RelayOne);
                            goto case 1;
                        case 3:
                            StartDeviceWorkers(link.RelayTwo);
                            goto case 2;
                        case 4:
                            StartDeviceWorkers(link.RelayThree);
                            goto case 3;
                        case 5:
                            StartDeviceWorkers(link.RelayFour);
                            goto case 4;
                        default:
                            throw new NotSupportedException();
                    }

                    Thread.Sleep(randomizer.Next(10));
                }
            });
        }

        public void StopWorkers(Dictionary<string, Link> linkDatabase)
        {
            foreach (Link link in linkDatabase.Values)
            {
                switch (link.HopCount)
                {
                    case 0:
                        StopDeviceWorkers(link.BaseDevice);
                        break;
                    case 1:
                        StopDeviceWorkers(link.EndDevice);
                        goto case 0;
                    case 2:
                        StopDeviceWorkers(link.RelayOne);
                        goto case 1;
                    case 3:
                        StopDeviceWorkers(link.RelayTwo);
                        goto case 2;
                    case 4:
                        StopDeviceWorkers(link.RelayThree);
                        goto case 3;
                    case 5:
                        StopDeviceWorkers(link.RelayFour);
                        goto case 4;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public void InitDeviceWorkers(Device device)
        {
            device.CollectorSysName = new SnmpSysName(device);
            device.CollectorUptime = new SnmpUptime(device);
            device.CollectorSignal = new SnmpSignal(device);
            device.CollectorSignalQ = new SnmpSignalQ(device);
            if (device.IsEnabledTx)
                device.CollectorTx = new SnmpTx(device);
            if (device.IsEnabledRx)
                device.CollectorRx = new SnmpRx(device);
            device.CollectorPing = new PingCollector(device);
            StartDeviceWorkers(device);
        }

        public void StartDeviceWorkers(Device device)
        {
            device.CollectorSysName.Start();
            device.CollectorUptime.Start();
            device.CollectorSignal.Start();
            device.CollectorSignalQ.Start();
            if (device.IsEnabledTx)
                device.CollectorTx.Start();
            if (device.IsEnabledRx)
                device.CollectorRx.Start();
            device.CollectorPing.Start();

            device.CollectorSignal.StartStatistic();
            device.CollectorSignalQ.StartStatistic();
            device.CollectorPing.StartStatistic();
        }

        public void StopDeviceWorkers(Device device)
        {
            device.CollectorSysName.Stop();
            device.CollectorUptime.Stop();
            device.CollectorSignal.Stop();
            device.CollectorSignalQ.Stop();
            if (device.IsEnabledRx)
                device.CollectorTx.Stop();
            if (device.IsEnabledRx)
                device.CollectorRx.Stop();
            device.CollectorPing.Stop();
        }
    }
}
