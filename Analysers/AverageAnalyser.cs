using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrowaveMonitor.Database;
using Vibrant.InfluxDB.Client.Rows;
using Vibrant.InfluxDB.Client;
using Microsoft.CSharp.RuntimeBinder;

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
        public int LongLimit { get; set; }
        public int ShortLimit { get; set; }
        public PercentDiff Percentages { get; set; }

        private readonly Dictionary<int, double> dataSignalAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataSignalQAvg = new Dictionary<int, double>();
        //private readonly Dictionary<int, double> dataTempIduAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataVoltageAvg = new Dictionary<int, double>();
        private readonly Dictionary<int, double> dataPingAvg = new Dictionary<int, double>();
        private readonly object dataLocker = new object();

        private readonly Dictionary<int, int> idsSignal = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsSignalQ = new Dictionary<int, int>();
        //private readonly Dictionary<int, int> idsTempIdu = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsVoltage = new Dictionary<int, int>();
        private readonly Dictionary<int, int> idsPing = new Dictionary<int, int>();

        private static readonly Dictionary<int, bool> isIndicatedSignal = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedSignalQ = new Dictionary<int, bool>();
        //private static readonly Dictionary<int, bool> isIndicatedTempIdu = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedVoltage = new Dictionary<int, bool>();
        private static readonly Dictionary<int, bool> isIndicatedPing = new Dictionary<int, bool>();
        private static readonly object indiLocker = new object();

        private Thread tComparator;
        private readonly AlarmType alarmType;
        private readonly string longRetention;
        private readonly string longValueName;

        public AverageAnalyser(AlarmManager alarmManager, DataManager dataManager, int refreshInt, int compareInt, int longLim, int shortLim, PercentDiff percentDiff, AlarmType alarmType) : base(alarmManager, dataManager)
        {
            RefreshInterval = TimeSpan.FromMilliseconds(refreshInt);
            CompareInterval = TimeSpan.FromMilliseconds(compareInt);
            LongLimit = longLim;
            ShortLimit = shortLim;
            Percentages = percentDiff;
            this.alarmType = alarmType;

            if (shortLim > DataManager.contQueryCycle * 2)
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
            tQueryer = new Thread(LongAverage);
            tComparator = new Thread(ShortAverage);
            tQueryer.Start();
            tComparator.Start();
        }

        protected override void Stop()
        {
            base.Stop();
            tComparator.Abort();
        }

        private void LongAverage()
        {
            while (IsRunning)
            {
                DateTime start = DateTime.Now;

                GetLongAvg(DataManager.measSig, WatchSignal, dataSignalAvg);
                GetLongAvg(DataManager.measSigQ, WatchSignalQ, dataSignalQAvg);
                //GetLongAvg(DataManager.measTmpI, WatchTempIdu, dataTempIduAvg);
                GetLongAvg(DataManager.measVolt, WatchVoltage, dataVoltageAvg);
                GetLongAvg(DataManager.measLat, WatchPing, dataPingAvg);

                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(RefreshInterval - diff);
            }
        }

        private async void GetLongAvg(string meas, Dictionary<int, bool> watchInfo, Dictionary<int, double> data)
        {
            string query = $@"SELECT mean(""{longValueName}"") FROM ""{DataManager.databaseName}"".""{longRetention}"".""{meas}"" WHERE time > now() - {LongLimit}ms AND time < now() - {ShortLimit}ms GROUP BY ""device"" FILL(none)";

            List<InfluxSeries<DynamicInfluxRow>> series = await dataMan.QuerySeries(query);

            if (series != null)
                lock (watchLocker)
                {
                    lock (dataLocker)
                        foreach (InfluxSeries<DynamicInfluxRow> ser in series)
                        {
                            int devId = Convert.ToInt32(ser.GroupedTags.Values.First());
                            double value = Convert.ToDouble(ser.Rows.First().Fields.First().Value);

                            if (watchInfo.ContainsKey(devId))
                            {
                                if (data.ContainsKey(devId))
                                    data[devId] = value;
                                else
                                    data.Add(devId, value);
                            }
                        }
                }
        }

        private void ShortAverage()
        {
            Thread.Sleep(CompareInterval);
            while (IsRunning)
            {
                DateTime start = DateTime.Now;

                CompareAvg(DataManager.measSig, idsSignal, Measurement.Strength, Percentages.Signal, dataSignalAvg, isIndicatedSignal);
                CompareAvg(DataManager.measSigQ, idsSignalQ, Measurement.Quality, Percentages.SignalQ, dataSignalQAvg, isIndicatedSignalQ);
                //CompareAvg(DataManager.measTmpI, idsTempIdu, Measurement.TempIDU, Percentages.TempIdu, dataTempIduAvg, isIndicatedTempIdu);
                CompareAvg(DataManager.measVolt, idsVoltage, Measurement.Voltage, Percentages.Voltage, dataVoltageAvg, isIndicatedVoltage);
                CompareAvg(DataManager.measLat, idsPing, Measurement.Latency, Percentages.Latency, dataPingAvg, isIndicatedPing);

                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(CompareInterval - diff);
            }
        }

        private async void CompareAvg(string meas, Dictionary<int, int> ids, Measurement measure, double percentage, Dictionary<int, double> data, Dictionary<int, bool> indication)
        {
            string query = $@"SELECT mean(""{DataManager.defaultValueName}"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{meas}"" WHERE time > now() - {ShortLimit}ms AND time < now() GROUP BY ""device"" FILL(none)";

            List<InfluxSeries<DynamicInfluxRow>> series = await dataMan.QuerySeries(query);

            if (series != null)
                lock (dataLocker)
                    foreach (InfluxSeries<DynamicInfluxRow> ser in series)
                    {
                        int devId = Convert.ToInt32(ser.GroupedTags.Values.First());
                        double value = Convert.ToDouble(ser.Rows.First().Fields.First().Value);

                        if (data.ContainsKey(devId))
                        {
                            double diff = data[devId] - value;
                            double maxDiff = data[devId] * percentage;

                            if (Math.Abs(diff) > maxDiff)
                            {
                                if (!ids.ContainsKey(devId))
                                {
                                    int id;

                                    if (diff > 0)
                                        //id = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Warning, measure, alarmType, false, value);
                                        id = CheckedGenerate(devId, measure, false, value, indication);
                                    else
                                        //id = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Warning, measure, alarmType, true, value);
                                        id = CheckedGenerate(devId, measure, true, value, indication);

                                    ids.Add(devId, id);
                                }
                            }
                            else
                            {
                                if (ids.ContainsKey(devId))
                                {
                                    CheckedSettle(ids[devId], value, indication, devId);
                                    ids.Remove(devId);
                                }
                            }
                        }
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

                id = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Warning, measure, alarmType, trend, value);
                return id;
            }
            else
            {
                lock (indiLocker)
                    if (indication.ContainsKey(devId))
                        return 0;

                id = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Warning, measure, alarmType, trend, value);
                return id;
            }
        }

        private void CheckedSettle(int alarmId, double value, Dictionary<int, bool> indication, int devId)
        {
            if (alarmType == AlarmType.AvgLong)
                lock (indiLocker)
                    if (indication.ContainsKey(devId))
                        indication.Remove(devId);

            alarmMan.SettleAlarmDispatched(alarmId, value, false);
        }
    }
}
