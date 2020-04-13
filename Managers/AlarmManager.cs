using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;

namespace MicrowaveMonitor.Managers
{
    public class AlarmManager
    {
        private struct AlarmIdAssign
        {
            public byte count;
            public int ping;
            public int signal;
            public int signalQ;
            public int tempOdu;
            public int tempIdu;
            public int voltage;
        }

        private struct AlarmDisplay
        {
            public int id;
            public string rank;
            public string timestamp;
            public string link;
            public string device;
            public string measurement;
            public string problem;
            public string value;
        }

        public bool IsRunning { get; set; }

        private readonly LinkManager linkM;

        private readonly ObservableCollection<AlarmDisplay> alarmsCurrent = new ObservableCollection<AlarmDisplay>();
        private readonly ObservableCollection<AlarmDisplay> alarmsAck = new ObservableCollection<AlarmDisplay>();

        /* Device down detection */
        private readonly Dictionary<int, byte> downTriggers = new Dictionary<int, byte>();

        /* Treshold exceed detection */
        private readonly Dictionary<int, AlarmIdAssign> tresholdIds = new Dictionary<int, AlarmIdAssign>();


        public AlarmManager(LinkManager linkManager)
        {
            linkM = linkManager;
        }

        private int GenerateAlarm(int deviceId, AlarmRank rank, Measurement measure, AlarmType type, ValueTrend trend, double measValue)
        {
            return 0;
        }

        private void UpdateAlarm(int alarmId)
        {

        }

        private void SettleAlarm(int? alarmId, int? deviceId)
        {

        }

        public void DeviceDownTrigger(int deviceId)
        {
            lock (downTriggers)
            {
                if (downTriggers.ContainsKey(deviceId))
                    downTriggers[deviceId]++;
                else
                    downTriggers.Add(deviceId, 1);

                if (downTriggers[deviceId] == 1)
                    GenerateAlarm();
            }
        }

        public void DeviceUpTrigger(int deviceId)
        {
            lock (downTriggers)
            {
                if (downTriggers.ContainsKey(deviceId))
                {
                    downTriggers[deviceId]--;

                    if (downTriggers[deviceId] < 1)
                    {
                        SettleAlarm(null, deviceId);
                        downTriggers.Remove(deviceId);
                    }
                }
            }
        }

        public void TreshExcTrigger(int deviceId, Measurement measurement, double value)
        {
            int alarm = GenerateAlarm();
            AlarmIdAssign idAssign;

            lock (tresholdIds)
            {
                if (tresholdIds.ContainsKey(deviceId))
                    idAssign = tresholdIds[deviceId];
                else
                {
                    idAssign = new AlarmIdAssign();
                    idAssign.count = 0;
                    tresholdIds[deviceId] = idAssign;
                }

                switch (measurement)
                {
                    case Measurement.Ping:
                        idAssign.ping = alarm;
                        break;
                    case Measurement.Signal:
                        idAssign.signal = alarm;
                        break;
                    case Measurement.SignalQ:
                        idAssign.signalQ = alarm;
                        break;
                    case Measurement.TempOdu:
                        idAssign.tempOdu = alarm;
                        break;
                    case Measurement.TempIdu:
                        idAssign.tempIdu = alarm;
                        break;
                    case Measurement.Voltage:
                        idAssign.voltage = alarm;
                        break;
                    default:
                        return;
                }

                idAssign.count++;
            }
        }

        public void TreshSettTrigger(int deviceId, Measurement measurement, double value)
        {
            int alarm = 0;
            AlarmIdAssign ids;

            lock (tresholdIds)
            {
                if (tresholdIds.ContainsKey(deviceId))
                {
                    ids = tresholdIds[deviceId];
                    switch (measurement)
                    {
                        case Measurement.Ping:
                            alarm = ids.ping;
                            ids.ping = 0;
                            break;
                        case Measurement.Signal:
                            alarm = ids.signal;
                            ids.signal = 0;
                            break;
                        case Measurement.SignalQ:
                            alarm = ids.signalQ;
                            ids.signalQ = 0;
                            break;
                        case Measurement.TempOdu:
                            alarm = ids.tempOdu;
                            ids.tempOdu = 0;
                            break;
                        case Measurement.TempIdu:
                            alarm = ids.tempIdu;
                            ids.tempIdu = 0;
                            break;
                        case Measurement.Voltage:
                            alarm = ids.voltage;
                            ids.voltage = 0;
                            break;
                        default:
                            return;
                    }

                    ids.count--;
                    if (ids.count == 0)
                        tresholdIds.Remove(deviceId);
                }
            }

            SettleAlarm(alarm, null);
        }
    }
}
