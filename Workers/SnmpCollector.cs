using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Workers
{
    public abstract class SnmpCollector : Collector
    {
        protected ObjectIdentifier _collectedOid;
        protected int _refreshInterval;

        public ObjectIdentifier CollectedOid { get => _collectedOid; }
        public int RefreshInterval { get => _refreshInterval; }

        private Thread tCollector;

        public SnmpCollector(Device device) : base(device){}

        public void Start()
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

            tCollector = new Thread (() =>
            {
                while (IsRunning)
                {
                    beginTime = DateTime.Now;
                    try
                    {
                        var result = Messenger.Get
                        (
                            VersionCode.V1,
                            Device.Address,
                            Device.CommunityString,
                            new List<Variable> { new Variable(CollectedOid) },
                            timeout
                        );

                        finishTime = DateTime.Now;

                        Record(result, finishTime);

                        diffTime = finishTime - beginTime;
                        if (diffTime.TotalMilliseconds < RefreshInterval)
                            Thread.Sleep((int)(RefreshInterval - diffTime.TotalMilliseconds));
                    } catch (Lextm.SharpSnmpLib.Messaging.TimeoutException e)
                    {
                        /* TODO timeout log to events */
                        // Console.WriteLine(e.Message);
                    }
                }             
            });
            tCollector.Start();
        }

        public virtual void Record(IList<Variable> result, DateTime resultTime)
        {
            foreach (var item in result)
            {
                /* TODO default log to events */
                Console.WriteLine(item.Data.ToString());
            }
        }

        public override void Stop()
        {
            _isRunning = false;
            if (RefreshInterval > MaxTimeout)
                tCollector.Abort();
        }
    }
}
