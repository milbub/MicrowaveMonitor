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
    public class SnmpCollector
    {
        protected Device _device;
        protected ObjectIdentifier _collectedOid;
        protected uint _refreshInterval;
        protected bool _isRunning = false;

        public Device Device { get => _device; }
        public bool IsRunning { get => _isRunning; }

        public SnmpCollector(Device device)
        {
            _device = device;
        }

        public void Start()
        {
            DateTime beginTime;
            DateTime finishTime;
            TimeSpan diffTime;

            uint timeout;
            if (_refreshInterval > 10000)
                timeout = 10000;
            else
                timeout = _refreshInterval * 2;

            _isRunning = true;
            Task.Run(() =>
            {
                while (_isRunning)
                {
                    beginTime = DateTime.Now;
                    try
                    {
                        var result = Messenger.Get
                        (
                            VersionCode.V1,
                            _device.Address,
                            _device.CommunityString,
                            new List<Variable> { new Variable(_collectedOid) },
                            (int)timeout
                        );

                        finishTime = DateTime.Now;

                        Record(result, finishTime);

                        diffTime = finishTime - beginTime;
                        if (diffTime.TotalMilliseconds < _refreshInterval)
                            Thread.Sleep((int)(_refreshInterval - diffTime.TotalMilliseconds));
                    } catch (Lextm.SharpSnmpLib.Messaging.TimeoutException e)
                    {
                        /* TODO timeout log to events */
                        // Console.WriteLine(e.Message);
                    }
                }
            });
        }

        public virtual void Record(IList<Variable> result, DateTime resultTime)
        {
            foreach (var item in result)
            {
                /* TODO default log to events */
                Console.WriteLine(item.Data.ToString());
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
