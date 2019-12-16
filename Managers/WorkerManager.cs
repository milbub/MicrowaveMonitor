using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicrowaveMonitor.Workers;
using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Managers
{
    class WorkerManager
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

        public void StopWorkers(Dictionary<string, Link> linkDatabase)
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

        public void InitDeviceWorkers(Device device)
        {
            device.CollectorSysName = new SnmpSysName(device);
            device.CollectorUptime = new SnmpUptime(device);
            device.CollectorSignal = new SnmpSignal(device);
            device.CollectorSignalQ = new SnmpSignalQ(device);
            device.CollectorTx = new SnmpTx(device);
            device.CollectorRx = new SnmpRx(device);
            StartDeviceWorkers(device);
        }

        public void StartDeviceWorkers(Device device)
        {
            device.CollectorSysName.Start();
            device.CollectorUptime.Start();
            device.CollectorSignal.Start();
            device.CollectorSignalQ.Start();
            device.CollectorTx.Start();
            device.CollectorRx.Start();
        }

        public void StopDeviceWorkers(Device device)
        {
            device.CollectorSysName.Stop();
            device.CollectorUptime.Stop();
            device.CollectorSignal.Stop();
            device.CollectorSignalQ.Stop();
            device.CollectorTx.Stop();
            device.CollectorRx.Stop();
        }
    }
}
