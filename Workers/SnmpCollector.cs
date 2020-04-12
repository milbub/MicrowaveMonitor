﻿using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Threading;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Workers
{
    public abstract class SnmpCollector : Collector
    {
        public int Port { get; protected set; }
        public ObjectIdentifier CollectedOid { get; protected set; }
        public OctetString Community { get; protected set; }

        private static readonly TimeSpan messCyckle;
        private static int timeouts;
        private static DateTime lastMessage;

        static SnmpCollector()
        {
            messCyckle = TimeSpan.FromMinutes(5);
            timeouts = 0;
            lastMessage = DateTime.Now;
        }

        public SnmpCollector(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(address, deviceId, refreshInterval, display)
        {
            CollectedOid = new ObjectIdentifier(oid);
            Port = port;
            Community = new OctetString(community);
        }

        public override void Start()
        {
            if (IsRunning == false)
            {
                DateTime beginTime;
                DateTime finishTime;
                TimeSpan diffTime;

                int timeout;
                if (RefreshInterval > MaxTimeout)
                    timeout = MaxTimeout;
                else
                    timeout = RefreshInterval * 2;

                IsRunning = true;

                tCollector = new Thread(() =>
                {
                    while (IsRunning)
                    {
                        beginTime = DateTime.Now;
                        try
                        {
                            var result = Messenger.Get
                        (
                            VersionCode.V1,
                            new System.Net.IPEndPoint(Address, Port),
                            Community,
                            new List<Variable> { new Variable(CollectedOid) },
                            timeout
                        );

                            finishTime = DateTime.Now;

                            RecordData(result, finishTime);

                            diffTime = finishTime - beginTime;
                            if (diffTime.TotalMilliseconds < RefreshInterval)
                                Thread.Sleep((int)(RefreshInterval - diffTime.TotalMilliseconds));
                        }
                        catch (OperationException e)
                        {
                            if (e is Lextm.SharpSnmpLib.Messaging.TimeoutException)
                            {
                                TimeoutCounter();
                                continue;
                            }
                            if (e is ErrorException)
                            {
                                IsRunning = false;
                                Console.WriteLine("SNMP Error. Collector suspended. " + e.Message);
                                // TODO - exception handling
                            }
                            else
                            {
                                Thread.Sleep(RefreshInterval);
                                Console.WriteLine(e.Message);
                                // TODO - exception handling
                            }
                        }
                        catch (ThreadAbortException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                });
                tCollector.Start();
            }
        }

        private static void TimeoutCounter()
        {
            timeouts++;
            TimeSpan span = DateTime.Now - lastMessage;
            if (span > messCyckle)
            {
                lastMessage = DateTime.Now;
                Console.WriteLine("0" + timeouts.ToString() + " SNMP requests were lost during last " + span.Minutes.ToString() + " minutes and " + span.Seconds.ToString() + " seconds.");
                timeouts = 0;
            }
        }

        public abstract void RecordData(IList<Variable> result, DateTime resultTime);
    }
}
