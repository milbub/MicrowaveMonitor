using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;

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

        public SnmpCollector(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display, AlarmManager alarmManager, bool checkTresholds, float treshUp, float treshDown, Measurement measurement) : base(address, deviceId, refreshInterval, display, alarmManager, checkTresholds, treshUp, treshDown, measurement)
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

                IPEndPoint endPoint = new IPEndPoint(Address, Port);
                List<Variable> variables = new List<Variable> { new Variable(CollectedOid) };

                IsRunning = true;

                tCollector = new Thread(() =>
                {
                    while (IsRunning)
                    {
                        beginTime = DateTime.Now;
                        try
                        {
                            var result = Messenger.Get(VersionCode.V1, endPoint, Community, variables, timeout);

                            // null = timeout
                            if (result == null)
                            {
                                TimeoutCounter();
                                HasResponded(false);
                                continue;
                            }

                            finishTime = DateTime.Now;

                            RecordData(result, finishTime);
                            HasResponded(true);

                            diffTime = finishTime - beginTime;
                            if (diffTime.TotalMilliseconds < RefreshInterval)
                                Thread.Sleep((int)(RefreshInterval - diffTime.TotalMilliseconds));
                        }
                        catch (OperationException e)
                        {
                            if (e is ErrorException ex)
                            {
                                if (ex.Body != null && ex.Body.Scope != null && ex.Body.Scope.Pdu != null && ex.Body.Scope.Pdu.ErrorStatus != null)
                                    if (ex.Body.Scope.Pdu.ErrorStatus.ToInt32() == 2)
                                    {
                                        IsRunning = false;
                                        Console.WriteLine("2SNMP OID not found! Collector: " + measureType.ToString() + "; device ID: " + DeviceId + ", IP: " + Address + ". Collector suspended. Please check device OID configuration.");
                                    }
                                else
                                {
                                    Console.WriteLine("SNMP Exception: " + e.Message + ". Collector: " + measureType.ToString() + "; device ID: " + DeviceId + ", IP: " + Address + ". Will try again in 10 seconds...");
                                    Thread.Sleep(10000);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Operation Exception: " + e.Message + ". Collector: " + measureType.ToString() + "; device ID: " + DeviceId + ", IP: " + Address + ". Will try again in 10 seconds...");
                                Thread.Sleep(10000);
                            }
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine("Socket Exception: " + e.Message + ". Collector: " + measureType.ToString() + "; device ID: " + DeviceId + ", IP: " + Address + ". Will try again in 10 seconds...");
                            Thread.Sleep(10000);
                        }
                        catch (ThreadAbortException)
                        { }
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

        protected abstract void RecordData(IList<Variable> result, DateTime resultTime);
    }
}
