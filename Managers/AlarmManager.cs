using MicrowaveMonitor.Analysers;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Properties;
using MicrowaveMonitor.Workers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

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
            public DateTime Timestamp { get; set; }
            public DateTime EndTimestamp { get; set; }
            public string Link { get; set; }
            public string Device { get; set; }
            public string Measurement { get; set; }
            public string Problem { get; set; }
            public string Value { get; set; }
            public string SettledValue { get; set; }
            public bool Ack { get; set; }
        }

        public struct AlarmTimes
        {
            public int linkId;
            public int devId;
            public bool isActive;
            public DateTime start;
            public DateTime end;
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                if (value)
                {
                    if (Settings.Default.a_enable_longavg)
                        longAverage.IsRunning = true;
                    if (Settings.Default.a_enable_shortavg)
                        shortAverage.IsRunning = true;
                    if (Settings.Default.a_enable_periodic)
                        periodicityAna.IsRunning = true;
                }
                else
                {
                    if (Settings.Default.a_enable_longavg)
                        longAverage.IsRunning = false;
                    if (Settings.Default.a_enable_shortavg)
                        shortAverage.IsRunning = false;
                    if (Settings.Default.a_enable_periodic)
                        periodicityAna.IsRunning = false;
                }
            }
        }

        private readonly LinkManager linkM;
        private readonly DataManager dataM;
        private readonly Dictionary<int, DeviceDisplay> displays;

        public readonly ObservableCollection<AlarmDisplay> alarmsCurrent = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsAck = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsSettledAck = new ObservableCollection<AlarmDisplay>();
        public readonly ObservableCollection<AlarmDisplay> alarmsSettledUnack = new ObservableCollection<AlarmDisplay>();

        public readonly ObservableCollection<AlarmDisplay> deviceAlarms = new ObservableCollection<AlarmDisplay>();
        private int viewedDevice = 0;
        private readonly object deviceAlarmsLocker = new object();

        /* Statistics for analysers */
        public readonly Dictionary<int, AlarmTimes> downTimes = new Dictionary<int, AlarmTimes>();
        private readonly object downTimesLocker = new object();

        /** Device down detection **/
        private readonly Dictionary<int, byte> downTriggers = new Dictionary<int, byte>();
        private readonly Dictionary<int, int> downIds = new Dictionary<int, int>();
        private readonly object downLocker = new object();

        /** Treshold exceed detection **/
        private readonly Dictionary<int, AlarmIdAssign> tresholdIds = new Dictionary<int, AlarmIdAssign>();

        /*** Average exceed analysers ***/
        private readonly AverageAnalyser longAverage;
        private readonly AverageAnalyser shortAverage;

        /*** Temperature weather-based analysers ***/
        private readonly TemperatureAnalyser oduTemperAna;
        private readonly TemperatureAnalyser iduTemperAna;

        /*** Periodicity analyser ***/
        private readonly PeriodicityAnalyser periodicityAna;

        public AlarmManager(DataManager dataManager, LinkManager linkManager, Dictionary<int, DeviceDisplay> deviceDisplays)
        {
            linkM = linkManager;
            dataM = dataManager;
            displays = deviceDisplays;

            SetCollectionViewSort(alarmsCurrent, "Timestamp", ListSortDirection.Descending);
            SetCollectionViewSort(alarmsAck, "Timestamp", ListSortDirection.Descending);
            SetCollectionViewSort(alarmsSettledAck, "EndTimestamp", ListSortDirection.Descending);
            SetCollectionViewSort(alarmsSettledUnack, "EndTimestamp", ListSortDirection.Descending);
            SetCollectionViewSort(deviceAlarms, "Timestamp", ListSortDirection.Descending);

            LoadAlarmsOnStart();

            longAverage = new AverageAnalyser(this, dataM, linkM, AlarmType.AvgLong);
            shortAverage = new AverageAnalyser(this, dataM, linkM, AlarmType.AvgShort);

            oduTemperAna = new TemperatureAnalyser(this, dataM, Analyser.WatchTempOdu, Measurement.TempODU);
            iduTemperAna = new TemperatureAnalyser(this, dataM, Analyser.WatchTempIduOut, Measurement.TempIDU);

            periodicityAna = new PeriodicityAnalyser(this, dataM);

            LoadSettings();
        }

        private void SetCollectionViewSort(ObservableCollection<AlarmDisplay> collection, string propertyName, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(collection);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(propertyName, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        public void LoadSettings()
        {
            AverageAnalyser.PercentDiff LongAvgPercentDiff = new AverageAnalyser.PercentDiff()
            {
                Signal = Settings.Default.a_longavg_percDiff_sig,
                SignalQ = Settings.Default.a_longavg_percDiff_sigQ,
                TempIdu = Settings.Default.a_longavg_percDiff_TmpI,
                Voltage = Settings.Default.a_longavg_percDiff_volt,
                Latency = Settings.Default.a_longavg_percDiff_late
            };

            AverageAnalyser.PercentDiff ShortAvgPercentDiff = new AverageAnalyser.PercentDiff()
            {
                Signal = Settings.Default.a_shortavg_percDiff_sig,
                SignalQ = Settings.Default.a_shortavg_percDiff_sigQ,
                TempIdu = Settings.Default.a_shortavg_percDiff_TmpI,
                Voltage = Settings.Default.a_shortavg_percDiff_volt,
                Latency = Settings.Default.a_shortavg_percDiff_late
            };

            longAverage.LoadSettings(LongAvgPercentDiff,
                Settings.Default.a_longavg_baseRefresh,
                Settings.Default.a_longavg_compareRefresh,
                Settings.Default.a_longavg_longLimit,
                Settings.Default.a_longavg_shortLimit);

            shortAverage.LoadSettings(ShortAvgPercentDiff,
                Settings.Default.a_shortavg_baseRefresh,
                Settings.Default.a_shortavg_compareRefresh,
                Settings.Default.a_shortavg_longLimit,
                Settings.Default.a_shortavg_shortLimit);

            TemperatureAnalyser.DefaultWeatherCoeffs coeffsClear = new TemperatureAnalyser.DefaultWeatherCoeffs()
            {
                clear = Settings.Default.a_temper_coeff_clear_clear,
                clouds = Settings.Default.a_temper_coeff_clear_cloud,
                atmosphere = Settings.Default.a_temper_coeff_clear_atmos,
                snow = Settings.Default.a_temper_coeff_clear_snow,
                rain = Settings.Default.a_temper_coeff_clear_rain,
                drizzle = Settings.Default.a_temper_coeff_clear_drizz,
                storm = Settings.Default.a_temper_coeff_clear_storm
            };

            TemperatureAnalyser.DefaultWeatherCoeffs coeffsClouds = new TemperatureAnalyser.DefaultWeatherCoeffs()
            {
                clear = Settings.Default.a_temper_coeff_cloud_clear,
                clouds = Settings.Default.a_temper_coeff_cloud_cloud,
                atmosphere = Settings.Default.a_temper_coeff_cloud_atmos,
                snow = Settings.Default.a_temper_coeff_cloud_snow,
                rain = Settings.Default.a_temper_coeff_cloud_rain,
                drizzle = Settings.Default.a_temper_coeff_cloud_drizz,
                storm = Settings.Default.a_temper_coeff_cloud_storm
            };

            oduTemperAna.LoadSettings(coeffsClear, coeffsClouds,
                Settings.Default.a_temper_debug,
                Settings.Default.a_temper_percDiff,
                Settings.Default.a_temper_degreesWind,
                Settings.Default.a_temper_maxAge,
                Settings.Default.a_temper_backDays,
                Settings.Default.a_temper_skippedDays,
                Settings.Default.a_temper_averageDays);

            iduTemperAna.LoadSettings(coeffsClear, coeffsClouds,
                Settings.Default.a_temper_debug,
                Settings.Default.a_temper_percDiff,
                Settings.Default.a_temper_degreesWind,
                Settings.Default.a_temper_maxAge,
                Settings.Default.a_temper_backDays,
                Settings.Default.a_temper_skippedDays,
                Settings.Default.a_temper_averageDays);

            PeriodicityAnalyser.PercentDiff PeriodicityPercentDiff = new Analyser.PercentDiff()
            {
                Signal = Settings.Default.a_periodic_percDiff_sig,
                SignalQ = Settings.Default.a_periodic_percDiff_sigQ,
                Voltage = Settings.Default.a_periodic_percDiff_volt
            };

            periodicityAna.LoadSettings(PeriodicityPercentDiff,
                Settings.Default.a_periodic_debug);

            if (IsRunning)
            {
                longAverage.IsRunning = Settings.Default.a_enable_longavg;
                shortAverage.IsRunning = Settings.Default.a_enable_shortavg;
                periodicityAna.IsRunning = Settings.Default.a_enable_periodic;
            }
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceDisplay disp = (DeviceDisplay)sender;

            switch (e.PropertyName)
            {
                case "DataTempOdu":
                    if (Settings.Default.a_enable_temper)
                    {
                        if (disp.WeatherTemp is null || disp.WeatherId is null || disp.WeatherWind is null)
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLatitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double la))
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLongitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double lo))
                            break;
                        oduTemperAna.Compare(disp.Id, disp.DataTempOdu.Data, (float)disp.WeatherTemp, (int)disp.WeatherId, (double)disp.WeatherWind, la, lo);
                    }
                    break;
                case "DataTempIdu":
                    if (Settings.Default.a_enable_temper)
                    {
                        if (disp.WeatherTemp is null || disp.WeatherId is null || disp.WeatherWind is null)
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLatitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double lat))
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLongitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double lon))
                            break;
                        iduTemperAna.Compare(disp.Id, disp.DataTempIdu.Data, (float)disp.WeatherTemp, (int)disp.WeatherId, (double)disp.WeatherWind, lat, lon);
                    }
                    break;
                case "WeatherId":
                    if (Settings.Default.a_enable_temper)
                    {
                        if (disp.WeatherTemp is null || disp.WeatherId is null || disp.WeatherWind is null)
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLatitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double lati))
                            break;
                        if (!double.TryParse(WeatherCollector.DeviceLongitude[disp.Id], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out double longi))
                            break;
                        oduTemperAna.WeatherChanged(disp.Id, (int)disp.WeatherId, (double)disp.WeatherWind, lati, longi);
                        iduTemperAna.WeatherChanged(disp.Id, (int)disp.WeatherId, (double)disp.WeatherWind, lati, longi);
                    }
                    break;
                default:
                    return;
            }
        }

        public int GenerateAlarm(int deviceId, AlarmRank rank, Measurement measure, AlarmType method, bool trend, double measValue)
        {
            int link = linkM.FindLinkByDevice(deviceId);
            string deviceType = linkM.GetDeviceType(deviceId);
            string linkName = linkM.LinkNames[link];

            Alarm alarm = new Alarm()
            {
                Rank = rank,
                IsActive = true,
                IsAck = false,
                IsShowed = true,
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

            string text = TextFiller(method, trend, measValue);

            AlarmDisplay disp = new AlarmDisplay();
            if (method == AlarmType.Down || method == AlarmType.Repetition)
                disp.Value = "-";
            else
                disp.Value = measValue.ToString("0.00");
            disp.Id = alarm.Id;
            disp.Rank = rank.ToString();
            disp.Timestamp = alarm.GenerTime;
            disp.Link = linkName;
            disp.Device = deviceType;
            disp.Measurement = measure.ToString();
            disp.Problem = text;
            disp.Ack = false;

            lock (displays)
            {
                if ((int)displays[deviceId].State < (int)rank)
                    displays[deviceId].State = (DeviceDisplay.LinkState)(int)rank;
            }

            Console.WriteLine(((int)rank + 2).ToString() + " Link: " + linkName + "; Device: " + deviceType + "; Measure: " + measure.ToString() + ". " + text);

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (alarmsCurrent)
                    alarmsCurrent.Add(disp);

                if (viewedDevice == deviceId)
                    lock (deviceAlarmsLocker)
                        deviceAlarms.Add(MakeAlarmDisplay(alarm));
            }));

            return alarm.Id;
        }

        public void SettleAlarm(int alarmId, double settledValue, bool stopping)
        {
            if (alarmId == 0)
                return;

            Alarm alarm = linkM.GetAlarm(alarmId);
            string linkName = linkM.LinkNames[alarm.LinkId];

            alarm.IsActive = false;
            alarm.SettledTime = DateTime.Now;
            alarm.SettledValue = settledValue;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
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

                if (stopping)
                    display.SettledValue = "Stopped.";
                else if (alarm.Type == AlarmType.Down || alarm.Type == AlarmType.Repetition)
                    display.SettledValue = "-";
                else
                    display.SettledValue = settledValue.ToString("0.00");
                display.EndTimestamp = alarm.SettledTime;

                lock (destination)
                    destination.Add(display);
            }));

            linkM.UpdateAlarm(alarm);

            if (stopping)
                return;

            Console.WriteLine(7 + " Link: " + linkName + "; Device: " + alarm.DeviceType + "; Measure: " + alarm.Measure.ToString() + ".");

            DeviceDisplay.LinkState state = DeviceDisplay.LinkState.Running;
            IEnumerable<AlarmDisplay> activeCurrAlarms = alarmsCurrent.Where(a => a.Link == linkName && a.Device == alarm.DeviceType);
            IEnumerable<AlarmDisplay> activeAckAlarms = alarmsAck.Where(a => a.Link == linkName && a.Device == alarm.DeviceType);

            foreach (AlarmDisplay a in activeCurrAlarms)
            {
                if (a.Id == alarm.Id)
                    continue;

                int ar = (int)(TypeDescriptor.GetConverter(alarm.Rank).ConvertFrom(a.Rank));
                if (ar > (int)state)
                    state = (DeviceDisplay.LinkState)ar;
            }

            foreach (AlarmDisplay a in activeAckAlarms)
            {
                if (a.Id == alarm.Id)
                    continue;

                int ar = (int)(TypeDescriptor.GetConverter(alarm.Rank).ConvertFrom(a.Rank));
                if (ar > (int)state)
                    state = (DeviceDisplay.LinkState)ar;
            }

            lock (displays)
                displays[alarm.DeviceId].State = state;
        }

        private string TextFiller(AlarmType method, bool trend, double value)
        {
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
                case AlarmType.Repetition:
                    text = $"Values have repetitive character with period of {value:0.00} minutes.";
                    break;
                case AlarmType.TempCorrel:
                    if (trend)
                        text = "Temperature exceeded safe limit based on corresponding ambient temperature.";
                    else
                        text = "Temperature dropped below ambient temperature.";
                    break;
                default:
                    break;
            }

            return text;
        }

        private void LoadAlarmsOnStart()
        {
            TableQuery<Alarm> alarms = linkM.GetListedAlarmsTable();

            foreach (Alarm alarm in alarms.Where(v => v.IsActive))
            {
                alarm.SettledTime = DateTime.Now;
                alarm.SettledValue = 0;
                alarm.IsActive = false;
                linkM.UpdateAlarm(alarm);
            }

            DateTime limit = DateTime.Now - Properties.Settings.Default.a_longavg_longLimit;

            foreach (Alarm alarm in alarms)
            {
                if (alarm.IsShowed)
                {
                    AlarmDisplay disp = MakeAlarmDisplay(alarm);

                    if (alarm.IsAck)
                        lock (alarmsSettledAck)
                            alarmsSettledAck.Add(disp);
                    else
                        lock (alarmsSettledUnack)
                            alarmsSettledUnack.Add(disp);
                }

                if (alarm.Type == AlarmType.Down && alarm.GenerTime > limit)
                    AddToDownTimes(alarm, false);
            }
        }

        private AlarmDisplay MakeAlarmDisplay(Alarm alarm)
        {
            string text = TextFiller(alarm.Type, alarm.Trend, alarm.GenerValue);

            AlarmDisplay disp = new AlarmDisplay();

            if (alarm.Type == AlarmType.Down)
            {
                disp.Value = "-";
                disp.SettledValue = "-";
            }
            else
            {
                disp.Value = alarm.GenerValue.ToString("0.00");
                disp.SettledValue = alarm.SettledValue.ToString("0.00");
            }

            if (alarm.IsActive)
            {
                disp.EndTimestamp = DateTime.MinValue;
                disp.SettledValue = "ACTIVE";
            }
            else
            {
                disp.EndTimestamp = alarm.SettledTime;
            }

            disp.Id = alarm.Id;
            disp.Rank = alarm.Rank.ToString();
            disp.Timestamp = alarm.GenerTime;
            disp.Link = linkM.LinkNames[alarm.LinkId];
            disp.Device = alarm.DeviceType;
            disp.Measurement = alarm.Measure.ToString();
            disp.Problem = text;
            disp.Ack = alarm.IsAck;

            return disp;
        }

        private void AddToDownTimes(Alarm alarm, bool active)
        {
            DateTime virtualStart = alarm.GenerTime - TimeSpan.FromMilliseconds(Collector.MaxTimeoutCount * Collector.MaxTimeout);

            AlarmTimes times = new AlarmTimes()
            {
                linkId = alarm.LinkId,
                devId = alarm.DeviceId,
                isActive = active,
                start = virtualStart,
                end = alarm.SettledTime
            };
            lock (downTimesLocker)
                downTimes.Add(alarm.Id, times);
        }

        public void FillDeviceAlarms(int devId)
        {
            if (devId == viewedDevice)
                return;

            TableQuery<Alarm> alarms = linkM.GetDeviceAlarmsTable(devId, 500);

            deviceAlarms.Clear();
            viewedDevice = devId;

            lock (deviceAlarmsLocker)
                foreach (Alarm alarm in alarms)
                {
                    deviceAlarms.Add(MakeAlarmDisplay(alarm));
                }
        }

        public void RegisterListener(Device device)
        {
            lock (Analyser.watchLocker)
            {
                Analyser.WatchSignal.Add(device.Id, device.IsWatchedSignal);
                Analyser.WatchSignalQ.Add(device.Id, device.IsWatchedSignalQ);
                Analyser.WatchTempOdu.Add(device.Id, device.IsWatchedTempOdu);
                if (device.IsWatchedTempIdu)
                {
                    if (device.IsTempIduOutdoor)
                    {
                        Analyser.WatchTempIduOut.Add(device.Id, true);
                        Analyser.WatchTempIduIn.Add(device.Id, false);
                    }
                    else
                    {
                        Analyser.WatchTempIduOut.Add(device.Id, false);
                        Analyser.WatchTempIduIn.Add(device.Id, true);
                    }
                }
                else
                {
                    Analyser.WatchTempIduOut.Add(device.Id, false);
                    Analyser.WatchTempIduIn.Add(device.Id, false);
                }
                Analyser.WatchVoltage.Add(device.Id, device.IsWatchedVoltage);
                Analyser.WatchPing.Add(device.Id, device.IsWatchedPing);
            }

            displays[device.Id].PropertyChanged += DataChanged;
        }

        public void UnregisterListener(Device device)
        {
            displays[device.Id].PropertyChanged -= DataChanged;

            lock (Analyser.watchLocker)
            {
                Analyser.WatchSignal.Remove(device.Id);
                Analyser.WatchSignalQ.Remove(device.Id);
                Analyser.WatchTempOdu.Remove(device.Id);
                Analyser.WatchTempIduIn.Remove(device.Id);
                Analyser.WatchTempIduOut.Remove(device.Id);
                Analyser.WatchVoltage.Remove(device.Id);
                Analyser.WatchPing.Remove(device.Id);
            }
        }

        public void UpdateDeviceWatch(Device device)
        {
            lock (Analyser.watchLocker)
            {
                Analyser.WatchSignal[device.Id] = device.IsWatchedSignal;
                Analyser.WatchSignalQ[device.Id] = device.IsWatchedSignalQ;
                Analyser.WatchTempOdu[device.Id] = device.IsWatchedTempOdu;
                if (device.IsWatchedTempIdu)
                {
                    if (device.IsTempIduOutdoor)
                    {
                        Analyser.WatchTempIduOut[device.Id] = true;
                        Analyser.WatchTempIduIn[device.Id] = false;
                    }
                    else
                    {
                        Analyser.WatchTempIduOut[device.Id] = false;
                        Analyser.WatchTempIduIn[device.Id] = true;
                    }
                }
                else
                {
                    Analyser.WatchTempIduOut[device.Id] = false;
                    Analyser.WatchTempIduIn[device.Id] = false;
                }
                Analyser.WatchVoltage[device.Id] = device.IsWatchedVoltage;
                Analyser.WatchPing[device.Id] = device.IsWatchedPing;
            }
        }

        public void DeviceStopped(int id)
        {
            while (DeviceUpTrigger(id, true))
                DeviceUpTrigger(id, true);

            foreach (Measurement measure in (Measurement[])Enum.GetValues(typeof(Measurement)))
            {
                TreshSettTrigger(id, measure, 0, true);
            }

            longAverage.DeviceStopped(id);
            shortAverage.DeviceStopped(id);
            iduTemperAna.DeviceStopped(id);
            oduTemperAna.DeviceStopped(id);
        }

        public void HideAlarm(int id, bool ack)
        {
            if (ack)
            {
                lock (alarmsSettledAck)
                {
                    AlarmDisplay display = alarmsSettledAck.First(v => (v.Id == id));
                    alarmsSettledAck.Remove(display);
                }
            }
            else
            {
                lock (alarmsSettledUnack)
                {
                    AlarmDisplay display = alarmsSettledUnack.First(v => (v.Id == id));
                    alarmsSettledUnack.Remove(display);
                }
            }

            Alarm alarm = linkM.GetAlarm(id);
            alarm.IsShowed = false;
            linkM.UpdateAlarm(alarm);
        }

        public void HideAll(bool ack)
        {
            Task.Run(() =>
            {
                ObservableCollection<AlarmDisplay> alarms;
                if (ack)
                    alarms = alarmsSettledAck;
                else
                    alarms = alarmsSettledUnack;

                lock (alarms)
                {
                    foreach (AlarmDisplay disp in alarms)
                    {
                        Alarm alarm = linkM.GetAlarm(disp.Id);
                        alarm.IsShowed = false;
                        linkM.UpdateAlarm(alarm);
                    }

                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        alarms.Clear();
                    }));
                }
            });
        }

        public void SetAck(int id, bool active, bool ack)
        {
            ObservableCollection<AlarmDisplay> now, intended;

            if (ack)
            {
                if (active)
                {
                    now = alarmsCurrent;
                    intended = alarmsAck;
                }
                else
                {
                    now = alarmsSettledUnack;
                    intended = alarmsSettledAck;
                }
            }
            else
            {
                if (active)
                {
                    now = alarmsAck;
                    intended = alarmsCurrent;
                }
                else
                {
                    now = alarmsSettledAck;
                    intended = alarmsSettledUnack;
                }
            }

            AlarmDisplay display;

            lock (now)
            {
                display = now.First(v => (v.Id == id));
                now.Remove(display);
                ICollectionView dv = CollectionViewSource.GetDefaultView(now);
                dv.Refresh();
            }

            display.Ack = !display.Ack;

            lock (intended)
                intended.Add(display);

            Alarm alarm = linkM.GetAlarm(id);
            alarm.IsAck = !alarm.IsAck;
            linkM.UpdateAlarm(alarm);
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
                    int id = GenerateAlarm(deviceId, AlarmRank.Down, Measurement.All, AlarmType.Down, false, 0);
                    downIds.Add(deviceId, id);

                    AddToDownTimes(linkM.GetAlarm(id), true);
                }
            }
        }

        public bool DeviceUpTrigger(int deviceId, bool stopping)
        {
            lock (downLocker)
            {
                if (downTriggers.ContainsKey(deviceId))
                {
                    downTriggers[deviceId]--;

                    if (downTriggers[deviceId] < 1)
                    {
                        downTriggers.Remove(deviceId);
                        SettleAlarm(downIds[deviceId], 0, stopping);

                        lock (downTimesLocker)
                        {
                            AlarmTimes times = downTimes[downIds[deviceId]];
                            times.end = DateTime.Now;
                            times.isActive = false;
                            downTimes[downIds[deviceId]] = times;
                        }

                        downIds.Remove(deviceId);

                        return false;
                    }

                    return true;
                }
                else
                    return false;
            }
        }

        /////////////////////////
        //* Treshold triggers *//

        public void TreshExcTrigger(int deviceId, Measurement measurement, double value, bool trend)
        {
            int alarmId = GenerateAlarm(deviceId, AlarmRank.Critical, measurement, AlarmType.Treshold, trend, value);

            lock (tresholdIds)
            {
                if (!tresholdIds.ContainsKey(deviceId))
                    tresholdIds.Add(deviceId, new AlarmIdAssign() { });

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

        public void TreshSettTrigger(int deviceId, Measurement measurement, double value, bool stopping)
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

                    if (alarmId > 0)
                        ids.count--;

                    if (ids.count == 0)
                        tresholdIds.Remove(deviceId);
                    else
                        tresholdIds[deviceId] = ids;
                }
            }

            if (alarmId > 0)
                SettleAlarm(alarmId, value, stopping);
        }
    }
}
