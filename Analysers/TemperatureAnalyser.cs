using Itenso.TimePeriod;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;
using CoordinateSharp;
using OpenWeatherApi;
using System.Runtime.InteropServices;

namespace MicrowaveMonitor.Analysers
{
    public class TemperatureAnalyser
    {
        public struct DefaultWeatherCoeffs
        {
            public float clouds;
            public float clear;
            public float atmosphere;
            public float snow;
            public float rain;
            public float drizzle;
            public float storm;
        }

        private struct TimeVal
        {
            public DateTime? dateTime;
            public double? value;
        }

        private readonly AlarmManager alarmMan;
        private readonly DataManager dataMan;
        private readonly Dictionary<int, bool> isWatched;

        private const int millisOfDay = 86400000;

        public double TolerancePerc { get; set; }
        public double DegressPerWindMeter { get; set; }
        public TimeSpan MaxAge { get; set; }
        public int BackDaysCount { get; set; }
        public int SkippedDaysCount { get; set; } = 1;
        public Measurement Measure { get; private set; }
        public DefaultWeatherCoeffs CoeffsClear { get; set; }
        public DefaultWeatherCoeffs CoeffsClouds { get; set; }

        private readonly string measureName;

        private readonly Dictionary<int, double> deviceCoefficients = new Dictionary<int, double>();
        private readonly Dictionary<int, double> deviceWinds = new Dictionary<int, double>();
        private readonly Dictionary<int, DateTime> lastUpdate = new Dictionary<int, DateTime>();
        private readonly object coefficientsLocker = new object();

        private readonly Dictionary<int, int> ids = new Dictionary<int, int>();
        private readonly object idsLocker = new object();

        public TemperatureAnalyser(AlarmManager alarmManager, DataManager dataManager, Dictionary<int, bool> watched, Measurement measure, double percentDiff, double degressPerWindMeter, TimeSpan maxAge, int backDaysCount, int skippedDaysCount, DefaultWeatherCoeffs coeffsClear, DefaultWeatherCoeffs coeffsClouds)
        {
            alarmMan = alarmManager;
            dataMan = dataManager;
            isWatched = watched;

            TolerancePerc = percentDiff;
            DegressPerWindMeter = degressPerWindMeter;
            MaxAge = maxAge;
            BackDaysCount = backDaysCount;
            SkippedDaysCount = skippedDaysCount;
            Measure = measure;
            CoeffsClear = coeffsClear;
            CoeffsClouds = coeffsClouds;

            if (measure == Measurement.TempIDU)
                measureName = DataManager.measTmpI;
            else
                measureName = DataManager.measTmpO;
        }

        public void WeatherChanged(int devId, int weatherId, double wind, double latitude, double longitude)
        {
            lock (Analyser.watchLocker)
                if (isWatched.ContainsKey(devId))
                {
                    if (!isWatched[devId])
                        return;
                }
                else
                    return;

            Task update = WeatherUpdate(devId, weatherId, wind, latitude, longitude);
            update.Wait();
        }

        public void Compare(int devId, double temperature, double airTemp, int weatherId, double wind, double latitude, double longitude)
        {
            double coefficient;
            double recordedWind;
            DateTime last;

            lock (coefficientsLocker)
                if (deviceCoefficients.ContainsKey(devId))
                {
                    coefficient = deviceCoefficients[devId];
                    recordedWind = deviceWinds[devId];
                    last = lastUpdate[devId];
                }
                else
                    return;

            lock (Analyser.watchLocker)
                if (isWatched.ContainsKey(devId))
                {
                    if (!isWatched[devId])
                        return;
                }
                else
                    return; 

            if (DateTime.Now - last > MaxAge)
            {
                Task update = WeatherUpdate(devId, weatherId, wind, latitude, longitude);
                update.Wait();
            }

            double upperLimit = (airTemp * coefficient - WindTempCorrection(wind, recordedWind)) * TolerancePerc;

            if (temperature > upperLimit)
            {
                lock (idsLocker)
                    if (!ids.ContainsKey(devId))
                    {
                        int alarm = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Critical, Measure, AlarmType.TempCorrel, true, temperature);
                        ids.Add(devId, alarm);
                    }
            }
            else if (temperature < airTemp / TolerancePerc)
            {
                lock (idsLocker)
                    if (!ids.ContainsKey(devId))
                    {
                        int alarm = alarmMan.GenerateAlarmDispatched(devId, AlarmRank.Critical, Measure, AlarmType.TempCorrel, false, temperature);
                        ids.Add(devId, alarm);
                    }
            }
            else
            {
                lock (idsLocker)
                    if (ids.ContainsKey(devId))
                    {
                        alarmMan.SettleAlarmDispatched(ids[devId], temperature, false);
                        ids.Remove(devId);
                    }
            }
        }

        private async Task WeatherUpdate(int devId, int weatherId, double wind, double latitude, double longitude)
        {
            int originalWeather = weatherId;

            TimeVal selection = await GetBestTime(devId, weatherId, wind, DateTime.Now.TimeOfDay);

            if (selection.dateTime == null && (originalWeather < 801 || originalWeather == 803))
            {
                weatherId = 804;
                selection = await GetBestTime(devId, weatherId, wind, DateTime.Now.TimeOfDay);
            }

            if (selection.dateTime == null && originalWeather != 800)
            {
                weatherId = 800;
                selection = await GetBestTime(devId, weatherId, wind, DateTime.Now.TimeOfDay);
            }

            if (selection.dateTime == null && !(originalWeather < 801 || originalWeather == 803) && originalWeather != 804)
            {
                weatherId = 804;
                selection = await GetBestTime(devId, weatherId, wind, DateTime.Now.TimeOfDay);
            }

            if (selection.dateTime == null)
                selection = await GetBestTime(devId, 0, wind, DateTime.Now.TimeOfDay);

            if (selection.dateTime == null)
                return;                             // no comparable data exists, abort

            double alternateWeatherCoeff = 1;

            if (originalWeather != weatherId)       // try to compute difference coefficient of alternate weather in other time (if available)
            {
                bool failed = true;                 // if this procedure fails, static coefficients are selected in switch below

                string query = $@"SELECT mean(""{DataManager.meanWindName}"") AS ""{DataManager.meanWindName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measWeath}"" WHERE time > now() - {BackDaysCount * millisOfDay}ms AND time < now() - {SkippedDaysCount * millisOfDay}ms AND ""device""='{devId}' AND ""{DataManager.medianCondiName}""={originalWeather} GROUP BY time(10m) FILL(none)";

                List<DynamicInfluxRow> results = await dataMan.QueryRows(query);

                Coordinate coord = new Coordinate(latitude, longitude, (DateTime)selection.dateTime);
                bool isSunUp = coord.CelestialInfo.IsSunUp;

                if (results != null)
                {
                    if (results.Count > 0)
                    {
                        for (int i = results.Count - 1; i >= 0; i--)
                        {
                            if (results[i].Timestamp != null)
                            {
                                Coordinate testedCoord = new Coordinate(latitude, longitude, (DateTime)results[i].Timestamp);

                                if (testedCoord.CelestialInfo.IsSunUp == isSunUp)
                                    continue;
                            }

                            results.RemoveAt(i);
                        }

                        if (results.Count > 0)
                        {
                            DynamicInfluxRow best = GetBestRow(results, wind);

                            TimeVal compareX = new TimeVal { dateTime = (DateTime)best.Timestamp, value = Convert.ToDouble(best.Fields.Values.First()) };
                            TimeVal compareY = await GetBestTime(devId, weatherId, (double)compareX.value, ((DateTime)compareX.dateTime).TimeOfDay);

                            if (compareY.dateTime != null)
                            {
                                double ratioX = await GetTemperaturesRatio(devId, (DateTime)compareX.dateTime, 0);
                                double ratioY = await GetTemperaturesRatio(devId, (DateTime)compareY.dateTime, WindTempCorrection(compareX.value, compareY.value));

                                if (ratioX > 0 && ratioY > 0)
                                {
                                    alternateWeatherCoeff += ratioX - ratioY;
                                    failed = false;
                                }
                            }
                        }
                    }
                }

                if (failed)
                {
                    DefaultWeatherCoeffs coeffs;
                    
                    if (weatherId == 800)
                        coeffs = CoeffsClear;
                    else
                        coeffs = CoeffsClouds;
                    
                    switch (originalWeather)
                    {
                        case 800:
                            alternateWeatherCoeff = coeffs.clear;
                            break;
                        case int n when (n > 800):
                            alternateWeatherCoeff = coeffs.clouds;
                            break;
                        case int n when (n < 800 && n >= 700):
                            alternateWeatherCoeff = coeffs.atmosphere;
                            break;
                        case int n when (n < 700 && n >= 600):
                            alternateWeatherCoeff = coeffs.snow;
                            break;
                        case int n when (n < 600 && n >= 500):
                            alternateWeatherCoeff = coeffs.rain;
                            break;
                        case int n when (n < 400 && n >= 300):
                            alternateWeatherCoeff = coeffs.drizzle;
                            break;
                        case int n when (n < 300 && n >= 200):
                            alternateWeatherCoeff = coeffs.storm;
                            break;
                        default:
                            break;
                    }
                }
            }

            double ratio = await GetTemperaturesRatio(devId, (DateTime)selection.dateTime, WindTempCorrection(wind, selection.value)) * alternateWeatherCoeff;
            if (ratio == 0)
                return;

            lock (coefficientsLocker)
                if (deviceCoefficients.ContainsKey(devId))
                {
                    deviceCoefficients[devId] = ratio;
                    if (selection.value != null)
                        deviceWinds[devId] = (double)selection.value;
                    lastUpdate[devId] = DateTime.Now;
                }
                else
                {
                    deviceCoefficients.Add(devId, ratio);
                    if (selection.value != null)
                        deviceWinds.Add(devId, (double)selection.value);
                    lastUpdate.Add(devId, DateTime.Now);
                }

            Console.WriteLine("0" + devId + " " + ratio + " " + selection.value + " " + alternateWeatherCoeff);
        }

        private async Task<TimeVal> GetBestTime(int devId, int weatherId, double wind, TimeSpan searchedTimeOfDay)
        {
            List<DynamicInfluxRow> days = new List<DynamicInfluxRow>();

            string weatherClause = String.Empty;
            if (weatherId != 0)
                weatherClause = $@"AND ""{DataManager.medianCondiName}""={weatherId} ";

            DateTime searchedTime = DateTime.Today + searchedTimeOfDay;

            for (int i = 1; i <= BackDaysCount; i++)
            {
                string query = $@"SELECT mean(""{DataManager.meanWindName}"") AS ""{DataManager.meanWindName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measWeath}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime)} - {(SkippedDaysCount + i) * millisOfDay + MaxAge.TotalMilliseconds}ms AND time < {DataManager.TimeToInfluxTime(searchedTime)} - {(SkippedDaysCount + i) * millisOfDay - MaxAge.TotalMilliseconds}ms AND ""device""='{devId}' {weatherClause}GROUP BY time(10m) FILL(none)";

                List<DynamicInfluxRow> results = await dataMan.QueryRows(query);

                if (results is null)
                    continue;
                else if (results.Count > 0)
                    days.Add(GetBestRow(results, wind));
            }

            double? selectedWind = null;
            DateTime? selectedTime = null;

            if (days.Count > 0)
            {
                DynamicInfluxRow best = GetBestRow(days, wind);
                selectedWind = Convert.ToDouble(best.Fields.First().Value);
                selectedTime = best.Timestamp;
            }

            return new TimeVal { dateTime = selectedTime, value = selectedWind };
        }

        private async Task<double> GetTemperaturesRatio(int devId, DateTime searchedTime, double measTempOffset)
        {
            string query = $@"SELECT mean(""{DataManager.meanValueName}"") AS ""{DataManager.meanValueName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measTmpA}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime - TimeSpan.FromMinutes(5))} AND time < {DataManager.TimeToInfluxTime(searchedTime + TimeSpan.FromMinutes(5))} AND ""device""='{devId}' FILL(linear)";
            
            DynamicInfluxRow row = await dataMan.QueryValue(query);

            if (row.Fields.Count > 0)
            {
                double valueAir = Convert.ToDouble(row.Fields.First().Value);

                query = $@"SELECT mean(""{DataManager.meanValueName}"") AS ""{DataManager.meanValueName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{measureName}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime - TimeSpan.FromMinutes(5))} AND time < {DataManager.TimeToInfluxTime(searchedTime + TimeSpan.FromMinutes(5))} AND ""device""='{devId}' FILL(linear)";

                row = await dataMan.QueryValue(query);

                if (row is null)
                    return 0;

                if (row.Fields.Count > 0)
                {
                    double x = Convert.ToDouble(row.Fields.First().Value);

                    if (valueAir > 0)
                    {
                        if (x - measTempOffset > valueAir)
                            x -= measTempOffset;
                        
                        return x / valueAir;
                    }
                }
            }
            
            return 0;
        }

        private DynamicInfluxRow GetBestRow(List<DynamicInfluxRow> inputResults, double wind)
        {
            DynamicInfluxRow best = null;
            double min = double.MaxValue;

            for (int i = 0; i < inputResults.Count; i++)
            {
                if (inputResults[i] != null)
                {
                    double diff = Math.Abs(Convert.ToDouble(inputResults[i].Fields.First().Value) - wind);

                    if (diff < min)
                    {
                        min = diff;
                        best = inputResults[i];
                    }
                }
            }

            return best;
        }

        private double WindTempCorrection(double? originalWind, double? newWind)
        {
            if (originalWind != null || newWind != null)
                return (double)(originalWind - newWind) * DegressPerWindMeter;
            else 
                return 0;
        }
    }
}
