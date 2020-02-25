using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MicrowaveMonitor.Workers
{
    public abstract class SnmpCollector : Collector
    {
        protected ObjectIdentifier _collectedOid;

        public ObjectIdentifier CollectedOid { get => _collectedOid; }

        private Thread tCollector;

        public SnmpCollector(Device device) : base(device)
        { }

        public override void Start()
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
                           new System.Net.IPEndPoint(Device.Address, Device.SnmpPort),
                           new OctetString(Device.CommunityString),
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
               }
            });
            tCollector.Start();
        }

        public override void Stop()
        {
            _isRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }

        public abstract void RecordData(IList<Variable> result, DateTime resultTime);
    }
}
