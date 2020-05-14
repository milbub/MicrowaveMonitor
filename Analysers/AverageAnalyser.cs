using Itenso.TimePeriod;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Analysers
{
    public class AverageAnalyser : Analyser
    {
        public struct PercentDiff
        {
            public double Signal { get; set; }
            public double SignalQ { get; set; }
            public double TempIdu { get; set; }
            public double Voltage { get; set; }
            public double Latency { get; set; }
        }

        public override TimeSpan RefreshInterval { get; set; }
        public TimeSpan CompareInterval { get; set; }
        public TimeSpan LongLimit { get; set; }
        public TimeSpan ShortLimit { get; set; }
        public PercentDiff Percentages { get; set; }

        private readonly Dictionary<int, double> dataSignalAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataSignalQAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataTempIduAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataVoltageAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataPingAvg = new Dictionary<int, double>();
        private readonly object dataLocker = new object();

        private readonly Dictionary<int, int> idsSignal = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsSignalQ = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsTempIdu = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsVoltage = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsPing = new Dictionary<int, int>();
        private readonly object idsLocker = new object();

        private static readonly Dictionary<int, bool> isIndicatedSignal = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedSignalQ = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedTempIdu = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedVoltage = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedPing = new Dictionary<int, bool>();
        private static readonly object indiLocker = new object();

        private readonly LinkManager linkMan;
        private readonly AlarmType alarmType;

        private Thread tComparator;
        private string longRetention;
        private string longValueName;

        public AverageAnalyser(AlarmManager alarmManager, DataManager dataManager, LinkManager linkManager, AlarmType alarm) : base(alarmManager, dataManager)
        {
            linkMan = linkManager;
            alarmType = alarm;
        }

        public void LoadSettings(PercentDiff percentDiff, TimeSpan refreshInt, TimeSpan compareInt, TimeSpan longLim, TimeSpan shortLim)
        {
            RefreshInterval = refreshInt;
            CompareInterval = compareInt;
            LongLimit = longLim;
            ShortLimit = shortLim;
            Percentages = percentDiff;

            if (shortLim.TotalMilliseconds > DataManager.contQueryCycle * 2)
            {
                longRetention = DataManager.retentionYear;
                longValueName = DataManager.meanValueName;
            }
            else
            {
                longRetention = DataManager.retentionWeek;
                longValueName = DataManager.defaultValueName;
            }
        }

        protected override void Start()
        {
            tQueryer = new Thread(LongAverage) { IsBackground = true, Name = "analyserAverage_queryer" };
            tComparator = new Thread(ShortAverage) { IsBackground = true, Name = "analyserAverage_comparator" };
            tQueryer.Start();
            tComparator.Start();
        }

        protected override void Stop()
        {
            base.Stop();
            tComparator.Abort();
        }

        private Dictionary<int, TimePeriodCollection> GetDownAffectedDevices(bool shortCompare)
        {
            Dictionary<int, TimePeriodCollection> linkDowns = new Dictionary<int, TimePeriodCollection>();
            DateTime now = DateTime.Now;

            foreach (AlarmManager.AlarmTimes downTime in alarmMan.downTimes.Values)
            {
                if (!linkDowns.ContainsKey(downTime.linkId))
                {
                    linkDowns.Add(downTime.linkId, new TimePeriodCollection());
                }

                if (downTime.isActive)
                    linkDowns[downTime.linkId].Add(new TimeRange(downTime.start, now));
                else
                    linkDowns[downTime.linkId].Add(new TimeRange(downTime.start, downTime.end));
            }

            Dictionary<int, TimePeriodCollection> devices = new Dictionary<int, TimePeriodCollection>();
            TimeGapCalculator<TimeRange> gapCalculator = new TimeGapCalculator<TimeRange>();
            CalendarTimeRange limit;
            if (shortCompare)
                limit = new CalendarTimeRange(now - ShortLimit, now);
            else
                limit = new CalendarTimeRange(now - LongLimit, ShortLimit);

            foreach (KeyValuePair<int, TimePeriodCollection> linkDownsPair in linkDowns)
            {
                TimePeriodCollection linkCorrectTimes = new TimePeriodCollection(gapCalculator.GetGaps(linkDownsPair.Value, limit));

                Link link = linkMan.GetLink(linkDownsPair.Key);

                if (link.DeviceBaseId > 0)
                    devices.Add(link.DeviceBaseId, linkCorrectTimes);
                if (link.DeviceEndId > 0)
                    devices.Add(link.DeviceEndId, linkCorrectTimes);
                if (link.DeviceR1Id > 0)
                    devices.Add(link.DeviceR1Id, linkCorrectTimes);
                if (link.DeviceR2Id > 0)
                    devices.Add(link.DeviceR2Id, linkCorrectTimes);
                if (link.DeviceR3Id > 0)
                    devices.Add(link.DeviceR3Id, linkCorrectTimes);
                if (link.DeviceR4Id > 0)
                    devices.Add(link.DeviceR4Id, linkCorrectTimes);
            }

            return devices;
        }

        private void LongAverage()
        {
            while (IsRunning)
            {
                DateTime start = DateTime.Now;

                Dictionary<int, TimePeriodCollection> affectedDevices = GetDownAffectedDevices(false);

                GetLongAvg(DataManager.measSig, WatchSignal, dataSignalAvg, affectedDevices);
                GetLongAvg(DataManager.measSigQ, WatchSignalQ, dataSignalQAvg, affectedDevices);
                GetLongAvg(DataManager.measTmpI, WatchTempIduIn, dataTempIduAvg, affectedDevices);
                GetLongAvg(DataManager.measVolt, WatchVoltage, dataVoltageAvg, affectedDevices);
                GetLongAvg(DataManager.measLat, WatchPing, dataPingAvg, affectedDevices);

                GetLongAvgAffected(DataManager.measSig, WatchSignal, dataSignalAvg, affectedDevices);
                GetLongAvgAffected(DataManager.measSigQ, WatchSignalQ, dataSignalQAvg, affectedDevices);
                GetLongAvgAffected(DataManager.measTmpI, WatchTempIduIn, dataTempIduAvg, affectedDevices);
                GetLongAvgAffected(DataManager.measVolt, WatchVoltage, dataVoltageAvg, affectedDevices);
                GetLongAvgAffected(DataManager.measLat, WatchPing, dataPingAvg, affectedDevices);

                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(RefreshInterval - diff);
            }
        }

        private async void GetLongAvg(string meas, Dictionary<int, bool> watchInfo, Dictionary<int, double> data, Dictionary<int, TimePeriodCollection> affected)
        {
            string except = String.Empty;

            foreach (int devId in affected.Keys)
            {
                except += $@"AND ""device""!='{devId}' ";
            }

            foreach (KeyValuePair<int, bool> info in watchInfo.Where(x => (x.Value == false)))
            {
                except += $@"AND ""device""!='{info.Key}' ";
            }

            string query = $@"SELECT mean(""{longValueName}"") FROM ""{DataManager.databaseName}"".""{longRetention}"".""{meas}"" WHERE time > now() - {LongLimit.TotalMilliseconds:0}ms AND time < now() - {ShortLimit.TotalMilliseconds:0}ms {except}GROUP BY ""device"" FILL(none)";

            List<InfluxSeries<DynamicInfluxRow>> series = await dataMan.QuerySeries(query);

            if (series != null)
                lock (dataLocker)
                    foreach (InfluxSeries<DynamicInfluxRow> ser in series)
                    {
                        int devId = Convert.ToInt32(ser.GroupedTags.Values.First());
                        double value = Convert.ToDouble(ser.Rows.First().Fields.First().Value);

                        WriteLongAvg(devId, value, data);
                    }
        }

        private async void GetLongAvgAffected(string meas, Dictionary<int, bool> watchInfo, Dictionary<int, double> data, Dictionary<int, TimePeriodCollection> affected)
        {
            foreach (KeyValuePair<int, TimePeriodCollection> dev in affected)
            {
                if (dev.Value.Count == 0)
                    continue;

                if (watchInfo.ContainsKey(dev.Key))
                    if (!watchInfo[dev.Key])
                        continue;

                string subqueries = String.Empty;

                bool next = false;
                foreach (var period in dev.Value)
                {
                    if (next)
                        subqueries += ", ";
                    subqueries += $@"(SELECT mean(""{longValueName}"") FROM ""{DataManager.databaseName}"".""{longRetention}"".""{meas}"" WHERE time > {DataManager.TimeToInfluxTime(period.Start)} AND time < {DataManager.TimeToInfluxTime(period.End)} AND ""device""='{dev.Key}' FILL(none))";
                    next = true;
                }

                string query = $@"SELECT mean(*) FROM {subqueries} FILL(none)";

                DynamicInfluxRow row = await dataMan.QueryValue(query);

                if (row != null)
                {
                    double value = Convert.ToDouble(row.Fields.First().Value);
                    lock (dataLocker)
                        WriteLongAvg(dev.Key, value, data);
                }
            }
        }

        private void WriteLongAvg(int devId, double value, Dictionary<int, double> data)
        {
            if (data.ContainsKey(devId))
                data[devId] = value;
            else
                data.Add(devId, value);
        }

        private void ShortAverage()
        {
            Thread.Sleep(CompareInterval);
            while (IsRunning)
            {
                DateTime start = DateTime.Now;

                Dictionary<int, TimePeriodCollection> affectedDevices = GetDownAffectedDevices(true);

                GetShortAvg(DataManager.measSig, idsSignal, Measurement.Strength, Percentages.Signal, dataSignalAvg, isIndicatedSignal, affectedDevices, WatchSignal);
                GetShortAvg(DataManager.measSigQ, idsSignalQ, Measurement.Quality, Percentages.SignalQ, dataSignalQAvg, isIndicatedSignalQ, affectedDevices, WatchSignalQ);
                GetShortAvg(DataManager.measTmpI, idsTempIdu, Measurement.TempIDU, Percentages.TempIdu, dataTempIduAvg, isIndicatedTempIdu, affectedDevices, WatchTempIduIn);
                GetShortAvg(DataManager.measVolt, idsVoltage, Measurement.Voltage, Percentages.Voltage, dataVoltageAvg, isIndicatedVoltage, affectedDevices, WatchVoltage);
                GetShortAvg(DataManager.measLat, idsPing, Measurement.Latency, Percentages.Latency, dataPingAvg, isIndicatedPing, affectedDevices, WatchPing);

                GetShortAvgAffected(DataManager.measSig, idsSignal, Measurement.Strength, Percentages.Signal, dataSignalAvg, isIndicatedSignal, affectedDevices, WatchSignal);
                GetShortAvgAffected(DataManager.measSigQ, idsSignalQ, Measurement.Quality, Percentages.SignalQ, dataSignalQAvg, isIndicatedSignalQ, affectedDevices, WatchSignalQ);
                GetShortAvgAffected(DataManager.measTmpI, idsTempIdu, Measurement.TempIDU, Percentages.TempIdu, dataTempIduAvg, isIndicatedTempIdu, affectedDevices, WatchTempIduIn);
                GetShortAvgAffected(DataManager.measVolt, idsVoltage, Measurement.Voltage, Percentages.Voltage, dataVoltageAvg, isIndicatedVoltage, affectedDevices, WatchVoltage);
                GetShortAvgAffected(DataManager.measLat, idsPing, Measurement.Latency, Percentages.Latency, dataPingAvg, isIndicatedPing, affectedDevices, WatchPing);

                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(CompareInterval - diff);
            }
        }

        private async void GetShortAvg(string meas, Dictionary<int, int> ids, Measurement measure, double percentage, Dictionary<int, double> data, Dictionary<int, bool> indication, Dictionary<int, TimePeriodCollection> affected, Dictionary<int, bool> watchInfo)
        {
            string except = String.Empty;

            foreach (int devId in affected.Keys)
            {
                except += $@"AND ""device""!='{devId}' ";
            }

            foreach (KeyValuePair<int, bool> info in watchInfo.Where(x => (x.Value == false)))
            {
                except += $@"AND ""device""!='{info.Key}' ";
            }

            string query = $@"SELECT mean(""{DataManager.defaultValueName}"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{meas}"" WHERE time > now() - {ShortLimit.TotalMilliseconds:0}ms AND time < now() {except}GROUP BY ""device"" FILL(none)";

            List<InfluxSeries<DynamicInfluxRow>> series = await dataMan.QuerySeries(query);

            if (series != null)
                lock (dataLocker)
                    foreach (InfluxSeries<DynamicInfluxRow> ser in series)
                    {
                        int devId = Convert.ToInt32(ser.GroupedTags.Values.First());
                        double value = Convert.ToDouble(ser.Rows.First().Fields.First().Value);

                        CompareAvg(devId, value, ids, measure, percentage, data, indication);
                    }
        }

        private async void GetShortAvgAffected(string meas, Dictionary<int, int> ids, Measurement measure, double percentage, Dictionary<int, double> data, Dictionary<int, bool> indication, Dictionary<int, TimePeriodCollection> affected, Dictionary<int, bool> watchInfo)
        {
            foreach (KeyValuePair<int, TimePeriodCollection> dev in affected)
            {
                if (dev.Value.Count == 0)
                    continue;

                if (watchInfo.ContainsKey(dev.Key))
                    if (!watchInfo[dev.Key])
                        continue;

                string subqueries = String.Empty;

                bool next = false;
                foreach (var period in dev.Value)
                {
                    if (next)
                        subqueries += ", ";
                    subqueries += $@"(SELECT mean(""{DataManager.defaultValueName}"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{meas}"" WHERE time > {DataManager.TimeToInfluxTime(period.Start)} AND time < {DataManager.TimeToInfluxTime(period.End)} AND ""device""='{dev.Key}' FILL(none))";
                    next = true;
                }

                string query = $@"SELECT mean(*) FROM {subqueries} FILL(none)";

                DynamicInfluxRow row = await dataMan.QueryValue(query);

                if (row != null)
                {
                    double value = Convert.ToDouble(row.Fields.First().Value);
                    lock (dataLocker)
                        CompareAvg(dev.Key, value, ids, measure, percentage, data, indication);
                }
            }
        }

        private void CompareAvg(int devId, double value, Dictionary<int, int> ids, Measurement measure, double percentage, Dictionary<int, double> data, Dictionary<int, bool> indication)
        {
            if (data.ContainsKey(devId))
            {
                double diff = data[devId] - value;
                double maxDiff = data[devId] * percentage;

                if (Math.Abs(diff) > maxDiff)
                {
                    lock (idsLocker)
                        if (!ids.ContainsKey(devId))
                        {
                            int id;

                            if (diff > 0)
                                id = CheckedGenerate(devId, measure, false, value, indication);
                            else
                                id = CheckedGenerate(devId, measure, true, value, indication);

                            ids.Add(devId, id);
                        }
                }
                else
                {
                    TrySettle(devId, ids, value, indication, false);
                }
            }
        }

        private void TrySettle(int devId, Dictionary<int, int> ids, double value, Dictionary<int, bool> indication, bool stopping)
        {
            lock (idsLocker)
                if (ids.ContainsKey(devId))
                {
                    CheckedSettle(ids[devId], value, indication, devId, stopping);
                    ids.Remove(devId);
                }
        }

        private int CheckedGenerate(int devId, Measurement measure, bool trend, double value, Dictionary<int, bool> indication)
        {
            int id;

            if (alarmType == AlarmType.AvgLong)
            {
                lock (indiLocker)
                    if (!indication.ContainsKey(devId))
                        indication.Add(devId, true);

                id = alarmMan.GenerateAlarm(devId, AlarmRank.Warning, measure, alarmType, trend, value);
                return id;
            }
            else
            {
                lock (indiLocker)
                    if (indication.ContainsKey(devId))
                        return 0;

                id = alarmMan.GenerateAlarm(devId, AlarmRank.Warning, measure, alarmType, trend, value);
                return id;
            }
        }

        private void CheckedSettle(int alarmId, double value, Dictionary<int, bool> indication, int devId, bool stopping)
        {
            if (alarmType == AlarmType.AvgLong)
                lock (indiLocker)
                    if (indication.ContainsKey(devId))
                        indication.Remove(devId);

            alarmMan.SettleAlarm(alarmId, value, stopping);
        }

        public void DeviceStopped(int devId)
        {
            TrySettle(devId, idsSignal, 0, isIndicatedSignal, true);
            TrySettle(devId, idsSignalQ, 0, isIndicatedSignalQ, true);
            TrySettle(devId, idsVoltage, 0, isIndicatedVoltage, true);
            TrySettle(devId, idsPing, 0, isIndicatedPing, true);
            TrySettle(devId, idsTempIdu, 0, isIndicatedTempIdu, true);
        }
    }
}
