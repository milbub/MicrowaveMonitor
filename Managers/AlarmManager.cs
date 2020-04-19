using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;

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

        public struct AlarmDisplay
        {
            public int Id { get; set; }
            public string Rank { get; set; }
            public string Timestamp { get; set; }
            public string EndTimestamp { get; set; }
            public string Link { get; set; }
            public string Device { get; set; }
            public string Measurement { get; set; }
            public string Problem { get; set; }
            public string Value { get; set; }
            public string SettledValue { get; set; }
            public bool Ack { get; set; }
        }

        public bool IsRunning { get; set; }

        private readonly LinkManager linkM;
        private readonly Dictionary<int, DeviceDisplay> displays;

        public readonly ObservableCollection<AlarmDisplay> alarmsCurrent = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsAck = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsSettledAck = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsSettledUnack = new ObservableCollection<AlarmDisplay>();

        /* Device down detection */
        private readonly Dictionary<int, byte> downTriggers = new Dictionary<int, byte>();
        private readonly Dictionary<int, int> downIds = new Dictionary<int, int>();
        private object downLocker = new object();

        /* Treshold exceed detection */
        private readonly Dictionary<int, AlarmIdAssign> tresholdIds = new Dictionary<int, AlarmIdAssign>();


        public AlarmManager(LinkManager linkManager, Dictionary<int, DeviceDisplay> deviceDisplays)
        {
            linkM = linkManager;
            displays = deviceDisplays;
        }

        private int GenerateAlarmDispatched(int deviceId, AlarmRank rank, Measurement measure, AlarmType method, bool trend, double measValue)
        {
            object obj = null;

            App.Current.Dispatcher.Invoke(delegate
            {
                return obj = GenerateAlarm(deviceId, rank, measure, method, trend, measValue);
            });

            if (obj == null)
                return 0;
            return (int)obj;
        }

        private void SettleAlarmDispatched(int alarmId, double settledValue, bool stopping)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                SettleAlarm(alarmId, settledValue, stopping);
            });
        }

        private int GenerateAlarm(int deviceId, AlarmRank rank, Measurement measure, AlarmType method, bool trend, double measValue)
        {
            int link = linkM.FindLinkByDevice(deviceId);
            string deviceType = linkM.GetDeviceType(deviceId);
            string linkName = linkM.LinkNames[link];

            Alarm alarm = new Alarm()
            {
                Rank = rank,
                IsActive = true,
                IsAck = false,
                GenerTime = DateTime.Now,
                LinkId = link,
                DeviceId = deviceId,
                Measure = measure,
                Type = method,
                Trend = trend,
                GenerValue = measValue,
                DeviceType = deviceType
            };

            linkM.AddAlarm(alarm);
            string text = string.Empty;

            switch (method)
            {
                case AlarmType.Down:
                    text = "Device is not responding.";
                    break;
                case AlarmType.Treshold:
                    if (trend)
                        text = "Value exceeded setted treshold.";
                    else
                        text = "Value dropped below setted treshold.";
                    break;
                case AlarmType.AvgLong:
                    if (trend)
                        text = "Value exceeded longterm average.";
                    else
                        text = "Value dropped below longterm average.";
                    break;
                case AlarmType.AvgShort:
                    if (trend)
                        text = "Value exceeded shortterm average.";
                    else
                        text = "Value dropped below shortterm average.";
                    break;
                case AlarmType.Retrospecitve:
                    if (trend)
                        text = "Value significantly exceeded values from the last days.";
                    else
                        text = "Value significantly dropped below values from the last days.";
                    break;
                case AlarmType.Repetition:
                    if (trend)
                        text = "Values are peaking on a regular basis.";
                    else
                        text = "Values ​​are dropping on a regular basis.";
                    break;
                case AlarmType.TempCorrel:
                    if (trend)
                        text = "The temperature is over the safe corresponding ambient temperature.";
                    else
                        text = "The temperature is below the safe corresponding ambient temperature.";
                    break;
                default:
                    break;
            }

            AlarmDisplay alarmDisplay = new AlarmDisplay();
            if (method == AlarmType.Down)
                alarmDisplay.Value = "-";
            else
                alarmDisplay.Value = measValue.ToString("0.00");
            alarmDisplay.Id = alarm.Id;
            alarmDisplay.Rank = rank.ToString();
            alarmDisplay.Timestamp = alarm.GenerTime.ToString("dd.MM.yyyy HH:mm:ss");
            alarmDisplay.Link = linkName;
            alarmDisplay.Device = deviceType;
            alarmDisplay.Measurement = measure.ToString();
            alarmDisplay.Problem = text;
            alarmDisplay.Ack = false;

            lock (displays)
            {
                if ((int)displays[deviceId].State < (int)rank)
                    displays[deviceId].State = (DeviceDisplay.LinkState)(int)rank;
            }

            Console.WriteLine(((int)rank + 1).ToString() + " Link: " + linkName + "; Device: " + deviceType + "; Measure: " + measure.ToString() + ". " + text);

            lock (alarmsCurrent)
                alarmsCurrent.Add(alarmDisplay);
            return alarm.Id;
        }

        private void SettleAlarm(int alarmId, double settledValue, bool stopping)
        {
            if (alarmId == 0)
                return;

            Alarm alarm = linkM.GetAlarm(alarmId);
            string linkName = linkM.LinkNames[alarm.LinkId];
            
            AlarmDisplay display;
            ObservableCollection<AlarmDisplay> destination;

            if (alarm.IsAck)
            {
                lock (alarmsAck)
                {
                    display = alarmsAck.First(v => (v.Id == alarm.Id));
                    alarmsAck.Remove(display);
                }
                destination = alarmsSettledAck;
            }
            else
            {
                lock (alarmsCurrent)
                {
                    display = alarmsCurrent.First(v => (v.Id == alarm.Id));
                    alarmsCurrent.Remove(display);
                }
                destination = alarmsSettledUnack;
            }

            alarm.IsActive = false;
            alarm.SettledTime = DateTime.Now;
            alarm.SettledValue = settledValue;

            if (stopping)
                display.SettledValue = "Stopped.";
            else if (alarm.Type == AlarmType.Down)
                display.SettledValue = "-";
            else
                display.SettledValue = settledValue.ToString("0.00");
            display.EndTimestamp = alarm.SettledTime.ToString("dd.MM.yyyy HH:mm:ss");

            lock (destination)
                destination.Add(display);
            linkM.UpdateAlarm(alarm);

            if (stopping)
                return;

            Console.WriteLine(6.ToString() + " Link: " + linkName + "; Device: " + alarm.DeviceType + "; Measure: " + alarm.Measure.ToString() + ".");

            /* TODO REWRITE !!! */
            lock (downTriggers)
                if (downTriggers.ContainsKey(alarm.DeviceId))
                    lock (displays)
                        displays[alarm.DeviceId].State = DeviceDisplay.LinkState.AlarmDown;
                else if (tresholdIds.ContainsKey(alarm.DeviceId))
                {
                    if (tresholdIds[alarm.DeviceId].count > 0)
                        lock (displays)
                            displays[alarm.DeviceId].State = DeviceDisplay.LinkState.AlarmCritical;
                }
                else
                    lock (displays)
                        displays[alarm.DeviceId].State = DeviceDisplay.LinkState.Running;
        }

        public void DeviceStopped(int id)
        {
            lock (downLocker)
            {
                if (downTriggers.ContainsKey(id))
                {
                    downTriggers.Remove(id);
                    SettleAlarmDispatched(downIds[id], 0, true);
                    downIds.Remove(id);
                }
            }
            
            foreach (Measurement measure in (Measurement[])Enum.GetValues(typeof(Measurement)))
            {
                TreshSettTrigger(id, measure, 0);
            }
        }

        public void SetAck(int id)
        {
            AlarmDisplay display;
            
            lock (alarmsCurrent)
            {
                display = alarmsCurrent.First(v => (v.Id == id));
                alarmsCurrent.Remove(display);
            }

            display.Ack = true;

            lock (alarmsAck)
                alarmsAck.Add(display);
        }

        public void UnsetAck(int id)
        {
            AlarmDisplay display;

            lock (alarmsAck)
            {
                display = alarmsAck.First(v => (v.Id == id));
                alarmsAck.Remove(display);
            }

            display.Ack = false;

            lock (alarmsCurrent)
                alarmsCurrent.Add(display);
        }

        /////////////////////////////
        //* Device down triggers *//

        public void DeviceDownTrigger(int deviceId)
        {
            lock (downLocker)
            {
                if (downTriggers.ContainsKey(deviceId))
                    downTriggers[deviceId]++;
                else
                {
                    downTriggers.Add(deviceId, 1);
                    downIds.Add(deviceId, GenerateAlarmDispatched(deviceId, AlarmRank.Down, Measurement.All, AlarmType.Down, false, 0));
                }
            }
        }

        public void DeviceUpTrigger(int deviceId)
        {
            lock (downLocker)
            {
                if (downTriggers.ContainsKey(deviceId))
                {
                    downTriggers[deviceId]--;

                    if (downTriggers[deviceId] < 1)
                    {
                        downTriggers.Remove(deviceId);
                        SettleAlarmDispatched(downIds[deviceId], 0, false);
                        downIds.Remove(deviceId);
                    }
                }
            }
        }

        /////////////////////////
        //* Treshold triggers *//

        public void TreshExcTrigger(int deviceId, Measurement measurement, double value, bool trend)
        {
            int alarmId = GenerateAlarmDispatched(deviceId, AlarmRank.Critical, measurement, AlarmType.Treshold, trend, value);

            lock (tresholdIds)
            {
                if (!tresholdIds.ContainsKey(deviceId))
                    tresholdIds.Add(deviceId, new AlarmIdAssign() {});

                AlarmIdAssign ids = tresholdIds[deviceId];

                switch (measurement)
                {
                    case Measurement.Latency:
                        ids.ping = alarmId;
                        break;
                    case Measurement.Strength:
                        ids.signal = alarmId;
                        break;
                    case Measurement.Quality:
                        ids.signalQ = alarmId;
                        break;
                    case Measurement.TempODU:
                        ids.tempOdu = alarmId;
                        break;
                    case Measurement.TempIDU:
                        ids.tempIdu = alarmId;
                        break;
                    case Measurement.Voltage:
                        ids.voltage = alarmId;
                        break;
                    default:
                        return;
                }

                ids.count++;
                tresholdIds[deviceId] = ids;
            }
        }

        public void TreshSettTrigger(int deviceId, Measurement measurement, double value)
        {
            int alarmId = 0;
            AlarmIdAssign ids;

            lock (tresholdIds)
            {
                if (tresholdIds.ContainsKey(deviceId))
                {
                    ids = tresholdIds[deviceId];
                    switch (measurement)
                    {
                        case Measurement.Latency:
                            alarmId = ids.ping;
                            ids.ping = 0;
                            break;
                        case Measurement.Strength:
                            alarmId = ids.signal;
                            ids.signal = 0;
                            break;
                        case Measurement.Quality:
                            alarmId = ids.signalQ;
                            ids.signalQ = 0;
                            break;
                        case Measurement.TempODU:
                            alarmId = ids.tempOdu;
                            ids.tempOdu = 0;
                            break;
                        case Measurement.TempIDU:
                            alarmId = ids.tempIdu;
                            ids.tempIdu = 0;
                            break;
                        case Measurement.Voltage:
                            alarmId = ids.voltage;
                            ids.voltage = 0;
                            break;
                        default:
                            return;
                    }

                    ids.count--;
                    if (ids.count == 0)
                        tresholdIds.Remove(deviceId);
                    else
                        tresholdIds[deviceId] = ids;
                }
            }

            if (alarmId > 0)
                SettleAlarmDispatched(alarmId, value, false);
        }
    }
}
