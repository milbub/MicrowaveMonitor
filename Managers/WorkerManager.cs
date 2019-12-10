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
        List<SnmpDataCollector> workers = new List<SnmpDataCollector>();

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
                            workers.Add(new SnmpDataUptime(link.BaseDevice));
                            workers.Add(new SnmpDataSignal(link.BaseDevice));
                            workers.Add(new SnmpDataSignalQ(link.BaseDevice));
                            workers.Add(new SnmpDataTx(link.BaseDevice));
                            workers.Add(new SnmpDataRx(link.BaseDevice));
                            break;
                        case 1:
                            workers.Add(new SnmpDataUptime(link.EndDevice));
                            workers.Add(new SnmpDataSignal(link.EndDevice));
                            workers.Add(new SnmpDataSignalQ(link.EndDevice));
                            workers.Add(new SnmpDataTx(link.EndDevice));
                            workers.Add(new SnmpDataRx(link.EndDevice));
                            goto case 0;
                        case 2:
                            workers.Add(new SnmpDataUptime(link.RelayOne));
                            workers.Add(new SnmpDataSignal(link.RelayOne));
                            workers.Add(new SnmpDataSignalQ(link.RelayOne));
                            workers.Add(new SnmpDataTx(link.RelayOne));
                            workers.Add(new SnmpDataRx(link.RelayOne));
                            goto case 1;
                        case 3:
                            workers.Add(new SnmpDataUptime(link.RelayTwo));
                            workers.Add(new SnmpDataSignal(link.RelayTwo));
                            workers.Add(new SnmpDataSignalQ(link.RelayTwo));
                            workers.Add(new SnmpDataTx(link.RelayTwo));
                            workers.Add(new SnmpDataRx(link.RelayTwo));
                            goto case 2;
                        case 4:
                            workers.Add(new SnmpDataUptime(link.RelayThree));
                            workers.Add(new SnmpDataSignal(link.RelayThree));
                            workers.Add(new SnmpDataSignalQ(link.RelayThree));
                            workers.Add(new SnmpDataTx(link.RelayThree));
                            workers.Add(new SnmpDataRx(link.RelayThree));
                            goto case 3;
                        case 5:
                            workers.Add(new SnmpDataUptime(link.RelayFour));
                            workers.Add(new SnmpDataSignal(link.RelayFour));
                            workers.Add(new SnmpDataSignalQ(link.RelayFour));
                            workers.Add(new SnmpDataTx(link.RelayFour));
                            workers.Add(new SnmpDataRx(link.RelayFour));
                            goto case 4;
                        default:
                            throw new NotSupportedException();
                    }

                    Thread.Sleep(randomizer.Next(10));
                }
            });
        }

        public void StopWorkers()
        {
            Task.Run(() =>
            {
                foreach (SnmpDataCollector collector in workers)
                {
                    collector.Stop();
                }
            });
        }
    }
}
