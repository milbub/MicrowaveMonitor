using Lextm.SharpSnmpLib;
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
        protected int _port;
        protected ObjectIdentifier _collectedOid;
        protected OctetString _community;

        public int Port { get => _port; }
        public ObjectIdentifier CollectedOid { get => _collectedOid; }
        public OctetString Community { get => _community; }

        public SnmpCollector(string oid, int port, string community, string address, int deviceId, int refreshInterval, DeviceDisplay display) : base(address, deviceId, refreshInterval, display)
        {
            _collectedOid = new ObjectIdentifier(oid);
            _port = port;
            _community = new OctetString(community);
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

                _isRunning = true;

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
                                continue;
                            if (e is ErrorException)
                            {
                                _isRunning = false;
                                Console.WriteLine("SNMP Error. Collector suspended.");
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

        public abstract void RecordData(IList<Variable> result, DateTime resultTime);
    }
}
