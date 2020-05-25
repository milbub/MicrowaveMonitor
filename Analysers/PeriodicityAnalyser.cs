using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;
using DSPLib;
using System.Numerics;

namespace MicrowaveMonitor.Analysers
{
    public class PeriodicityAnalyser : Analyser
    {
        public static bool DebugIsActive { get; set; }

        private static TimeSpan CalcTenMinInterval = TimeSpan.FromMinutes(10);
        private static TimeSpan CalcHourInterval = TimeSpan.FromMinutes(60);
        private static TimeSpan CalcDayInterval = TimeSpan.FromDays(1);
        private static TimeSpan CalcWeekInterval = TimeSpan.FromDays(7);

        private DateTime LastCalcTenMin = DateTime.MinValue;
        private DateTime LastCalcHour = DateTime.MinValue;
        private DateTime LastCalcDay = DateTime.MinValue;

        private readonly Dictionary<int, byte> idsType = new Dictionary<int, byte>();

        private const int fftElementCount = 128;    // count of series elements (rows) of which FFT is calculated, must be 2^n

        public PeriodicityAnalyser(AlarmManager alarmManager, DataManager dataManager) : base(alarmManager, dataManager)
        {
            RefreshInterval = CalcTenMinInterval + TimeSpan.FromSeconds(1);
        }

        public void LoadSettings(PercentDiff percentages, bool debug)
        {
            Percentages = percentages;
            DebugIsActive = debug;
        }

        protected override void Start()
        {
            tQueryer = new Thread(Run) { IsBackground = true, Name = "analyserPeriodicity" };
            tQueryer.Start();
        }

        private async void Run()
        {
            while (IsRunning)
            {
                DateTime start = DateTime.Now;

                if (DateTime.Now - CalcTenMinInterval > LastCalcTenMin)
                {
                    await Caller(CalcTenMinInterval, DataManager.defaultValueName, DataManager.retentionWeek, 1);
                    LastCalcTenMin = start;
                }

                if (DateTime.Now - CalcHourInterval > LastCalcHour)
                {
                    await Caller(CalcHourInterval, DataManager.defaultValueName, DataManager.retentionWeek, 2);
                    LastCalcHour = start;
                }

                if (DateTime.Now - CalcDayInterval > LastCalcDay)
                {
                    await Caller(CalcDayInterval, DataManager.meanValueName, DataManager.retentionMonth, 3);
                    await Caller(CalcWeekInterval, DataManager.meanValueName, DataManager.retentionYear, 4);
                    LastCalcDay = start;
                }

                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(RefreshInterval - diff);
            }
        }

        private async Task Caller(TimeSpan queryTimeSpan, string valueName, string retention, byte idType)
        {
            await Query(DataManager.measSig, Measurement.Strength, WatchSignal, idsSignal, queryTimeSpan, valueName, retention, Percentages.Signal, idType);
            await Query(DataManager.measSigQ, Measurement.Quality, WatchSignalQ, idsSignalQ, queryTimeSpan, valueName, retention, Percentages.SignalQ, idType);
            await Query(DataManager.measVolt, Measurement.Voltage, WatchVoltage, idsVoltage, queryTimeSpan, valueName, retention, Percentages.Voltage, idType);
        }

        private async Task Query(string measDb, Measurement measurement, Dictionary<int, bool> watchInfo, Dictionary<int, int> ids, TimeSpan queryTimeSpan, string valueName, string retention, double percentLimit, byte idType)
        {
            string except = String.Empty;

            foreach (KeyValuePair<int, bool> info in watchInfo.Where(x => (x.Value == false)))
            {
                except += $@"AND ""device""!='{info.Key}' ";
            }

            string query = $@"SELECT mean(""{valueName}"") FROM ""{DataManager.databaseName}"".""{retention}"".""{measDb}"" WHERE time > now() - {queryTimeSpan.TotalSeconds:0}s AND time < now() {except}GROUP BY time({(int)(queryTimeSpan.TotalMilliseconds / fftElementCount)}ms), ""device"" FILL(none)";

            List<InfluxSeries<DynamicInfluxRow>> series = await dataMan.QuerySeries(query);

            if (series != null)
            {
                string message = $"8Begin FFT meas: {measDb} time: {idType}";
                if (series.Count > 0)
                    if (series.First().Rows != null)
                        message += Environment.NewLine + $"query rows: {series.First().Rows.Count}";

                foreach (InfluxSeries<DynamicInfluxRow> ser in series)
                {
                    if (ser.Rows is null)
                        continue;

                    if (ser.Rows.Count < fftElementCount / 2)
                        continue;

                    double[] values = new double[fftElementCount];
                    int devId = Convert.ToInt32(ser.GroupedTags.Values.First());

                    if (ser.Rows.Count >= fftElementCount)
                    {
                        double last = 0;
                        for (int i = 0; i < fftElementCount; i++)
                        {
                            if (ser.Rows[i].Fields.Count > 0)
                            {
                                if (ser.Rows[i].Fields.First().Value != null)
                                {
                                    double value = Convert.ToDouble(ser.Rows[i].Fields.First().Value);
                                    values[i] = value;
                                    last = value;
                                }
                            }
                            else
                                values[i] = last;
                        }
                    }
                    else
                    {
                        double last = 0;
                        for (int i = 0; i < ser.Rows.Count; i++)
                        {
                            if (ser.Rows[i].Fields.Count > 0)
                            {
                                if (ser.Rows[i].Fields.First().Value != null)
                                {
                                    double value = Convert.ToDouble(ser.Rows[i].Fields.First().Value);
                                    values[i] = value;
                                    last = value;
                                }
                            }
                            else
                                values[i] = last;
                        }

                        for (int i = ser.Rows.Count; i < fftElementCount; i++)
                        {
                            values[i] = last;
                        }
                    }

                    double samplingRate = 1 / (queryTimeSpan.TotalSeconds / fftElementCount);     // sampling rate in Hz
                    double[] result = Calc(values, samplingRate);

                    if (DebugIsActive)
                        message += Environment.NewLine + $"FFT dev: {devId} meas: {measDb} time: {idType} {result[0]:0.000}/{result[1]:0.000}/{result[2]:0.00000} Hz";

                    if (result[0] * percentLimit < result[1])                                     // DC bin * percentage threshold check
                    {
                        lock (idsLocker)
                            if (!ids.ContainsKey(devId))
                            {
                                double period = (1 / result[2]) / 60;                             // period in minutes

                                int alarmId = alarmMan.GenerateAlarm(devId, AlarmRank.Info, measurement, AlarmType.Repetition, false, period);
                                ids.Add(devId, alarmId);
                                idsType.Add(devId, idType);
                            }
                    }
                    else
                    {
                        TrySettle(devId, ids, false, idType);
                    }
                }

                if (DebugIsActive)
                    Console.WriteLine(message);
            }
        }

        private double[] Calc(double[] values, double samplingRate)
        {
            FFT fft = new FFT();
            fft.Initialize((uint)values.Length);
            
            Complex[] cSpectrum = fft.Execute(values);                          // call the DFT and get the scaled spectrum back
            double[] rSpectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);     // convert the complex spectrum to real magnitude

            double[] freqSpan = fft.FrequencySpan(samplingRate);                // get spectrum frequency span

            int maxIndex = 3;                                                   // get the highest freq bin. Bin 0 is DC and it's omitted, low freqs bin 1 and bin 2 are omitted too for better results.
            double maxVal = rSpectrum[maxIndex];
            for (int i = 4; i < rSpectrum.Length; i++)
            {
                if (rSpectrum[i] > maxVal)
                {
                    maxVal = rSpectrum[i];
                    maxIndex = i;
                }
            }
                     // RETURN:     0:DC bin       1:highest bin     2:highest bin's freq
            return new double[] { rSpectrum[0], rSpectrum[maxIndex], freqSpan[maxIndex] };
        }

        public override void DeviceStopped(int devId)
        {
            TrySettle(devId, idsSignal, true, 0);
            TrySettle(devId, idsSignalQ, true, 0);
            TrySettle(devId, idsVoltage, true, 0);
        }

        private void TrySettle(int devId, Dictionary<int, int> ids, bool stopping, byte idType)
        {
            lock (idsLocker)
                if (ids.ContainsKey(devId))
                {
                    if (idsType[devId] == idType || idsType[devId] == 0)
                    {
                        alarmMan.SettleAlarm(ids[devId], 0, stopping);
                        ids.Remove(devId);
                        idsType.Remove(devId);
                    }
                }
        }
    }
}

