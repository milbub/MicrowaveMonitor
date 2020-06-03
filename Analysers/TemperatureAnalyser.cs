using CoordinateSharp;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

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

        private struct TimeWind
        {
            public DateTime? dateTime;
            public double? wind;
        }

        private readonly AlarmManager alarmMan;
        private readonly DataManager dataMan;
        private readonly Dictionary<int, bool> isWatched;

        private const int millisOfDay = 86400000;

        public static bool DebugIsActive { get; set; }

        public double TolerancePerc { get; set; }
        public double DegreesPerWindMeter { get; set; }
        public TimeSpan MaxAge { get; set; }
        public int BackDaysCount { get; set; }
        public int SkippedDaysCount { get; set; } = 1;
        public Measurement Measure { get; private set; }
        public DefaultWeatherCoeffs CoeffsClear { get; set; }
        public DefaultWeatherCoeffs CoeffsClouds { get; set; }
        public int AvgDaysCount { get; set; }
        public double UncertainDataCoeff { get; set; } = 1.3;
        public double MinimumDiff { get; set; }

        private readonly string measureName;

        private readonly Dictionary<int, double> deviceDifferences = new Dictionary<int, double>();
        private readonly Dictionary<int, double> deviceWinds = new Dictionary<int, double>();
        private readonly Dictionary<int, DateTime> lastUpdate = new Dictionary<int, DateTime>();
        private readonly object coefficientsLocker = new object();

        private readonly Dictionary<int, int> ids = new Dictionary<int, int>();
        private readonly object idsLocker = new object();

        public TemperatureAnalyser(AlarmManager alarmManager, DataManager dataManager, Dictionary<int, bool> watched, Measurement measure)
        {
            alarmMan = alarmManager;
            dataMan = dataManager;
            isWatched = watched;
            Measure = measure;

            if (measure == Measurement.TempIDU)
                measureName = DataManager.measTmpI;
            else
                measureName = DataManager.measTmpO;
        }

        public void LoadSettings(DefaultWeatherCoeffs coeffsClear, DefaultWeatherCoeffs coeffsClouds, bool debug, double percentDiff, double degreesPerWindMeter, TimeSpan maxAge, int backDaysCount, int skippedDaysCount, int averageDaysCount, double minimumDiff)
        {
            DebugIsActive = debug;
            TolerancePerc = percentDiff + 1;
            DegreesPerWindMeter = degreesPerWindMeter;
            MaxAge = maxAge;
            BackDaysCount = backDaysCount;
            SkippedDaysCount = skippedDaysCount;
            CoeffsClear = coeffsClear;
            CoeffsClouds = coeffsClouds;
            AvgDaysCount = averageDaysCount;
            MinimumDiff = minimumDiff;
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
            double diff;
            double recordedWind;
            DateTime last;

            lock (coefficientsLocker)
                if (deviceDifferences.ContainsKey(devId))
                {
                    diff = deviceDifferences[devId];
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

            double upperLimit = (airTemp + diff - WindTempCorrection(wind, recordedWind)) * TolerancePerc;
            // Console.WriteLine($"8 dev:{devId} {airTemp:0.00} {diff:0.00} {WindTempCorrection(wind, recordedWind):0.00} ... {upperLimit:0.00}");

            if (temperature > upperLimit)
            {
                lock (idsLocker)
                    if (!ids.ContainsKey(devId))
                    {
                        int alarm = alarmMan.GenerateAlarm(devId, AlarmRank.Critical, Measure, AlarmType.TempCorrel, true, temperature);
                        ids.Add(devId, alarm);
                        if (DebugIsActive)
                            Console.WriteLine($"8TA gener_a {measureName} dev: {devId} temper: {temperature:0.00000} thresh: {upperLimit:0.00000} diff:{diff:0.000}");
                    }
            }
            else if (temperature < airTemp / TolerancePerc)
            {
                lock (idsLocker)
                    if (!ids.ContainsKey(devId))
                    {
                        int alarm = alarmMan.GenerateAlarm(devId, AlarmRank.Critical, Measure, AlarmType.TempCorrel, false, temperature);
                        ids.Add(devId, alarm);
                        if (DebugIsActive)
                            Console.WriteLine($"8TA gener_a {measureName} dev: {devId} temper: {temperature:0.00000} thresh: {upperLimit:0.00000} diff:{diff:0.000}");
                    }
            }
            else
            {
                lock (idsLocker)
                    if (ids.ContainsKey(devId))
                    {
                        alarmMan.SettleAlarm(ids[devId], temperature, false);
                        ids.Remove(devId);
                        if (DebugIsActive)
                            Console.WriteLine($"8TA settle_a {measureName} dev: {devId} temper: {temperature:0.00000} thresh: {upperLimit:0.00000} diff:{diff:0.000}");
                    }
            }
        }

        public void DeviceStopped(int devId)
        {
            lock (idsLocker)
                if (ids.ContainsKey(devId))
                {
                    alarmMan.SettleAlarm(ids[devId], 0, true);
                    ids.Remove(devId);
                }
        }

        private async Task WeatherUpdate(int devId, int weatherId, double wind, double latitude, double longitude)
        {
            DateTime searchedTime = DateTime.Now;

            int[] ws;                           // array of substitute weathers
            double substitutionWeatherCoeff = 1;   // coefficient of weather substitute against original weather

            List<TimeWind> selection = await GetBestDays(devId, weatherId, wind, searchedTime.TimeOfDay);

            if (selection == null)              // if searched weather is not recently recorded, will try to find similar weather substitution
                switch (weatherId)
                {
                    case int n when (n <= 802 && n >= 800): // CLEAR SKY
                        ws = new int[] { 800, 801, 802 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        if (selection != null)
                            break;

                        ws = new int[] { 803, 804 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        substitutionWeatherCoeff = CoeffsClouds.clear;
                        break;
                    case int n when (n >= 803):             // CLOUDS
                        ws = new int[] { 803, 804 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        if (selection != null)
                            break;

                        ws = new int[] { 800, 801, 802 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        substitutionWeatherCoeff = CoeffsClear.clouds;
                        break;
                    case int n when (n < 800 && n >= 700):  // MISCELLANEOUS
                        ws = new int[] { 701, 711, 721, 731, 741, 751, 761, 762, 771, 781 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        break;
                    case int n when (n < 700 && n >= 600):  // SNOW
                        ws = new int[] { 600, 601, 602, 611, 612, 613, 615, 616, 620, 621, 622 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        break;
                    case int n when (n < 600 && n >= 500):  // RAIN
                        ws = new int[] { 500, 501, 502, 503, 504, 511, 520, 521, 522, 531 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        break;
                    case int n when (n < 400 && n >= 300):  // DRIZZLE
                        ws = new int[] { 300, 301, 302, 310, 311, 312, 313, 314, 321 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        break;
                    case int n when (n < 300 && n >= 200):  // THUNDERSTORM
                        ws = new int[] { 200, 201, 202, 210, 211, 212, 221, 230, 231, 232 };
                        selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                        break;
                    default:
                        break;
                }

            if (selection == null && weatherId < 800)   // if still not successful, try to substitute bad weather (below 800) with clouds or clear sky, then try to compute weather substitution coefficient
            {
                ws = new int[] { 803, 804 };
                selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);

                if (selection == null)
                {
                    ws = new int[] { 800, 801, 802 };
                    selection = await GetBestDays(devId, ws, wind, searchedTime.TimeOfDay);
                }

                if (selection != null)                  // try to compute the coefficient of the weather substitution in some other time (if available)
                {
                    bool failed = true;                 // if this procedure fails, static coefficients are selected in switch below

                    string query = $@"SELECT mean(""{DataManager.meanWindName}"") AS ""{DataManager.meanWindName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measWeath}"" WHERE time > now() - {BackDaysCount * millisOfDay}ms AND time < now() - {SkippedDaysCount * millisOfDay}ms AND ""device""='{devId}' AND ""{DataManager.medianCondiName}""={weatherId} GROUP BY time(10m) FILL(none)";

                    List<DynamicInfluxRow> results = await dataMan.QueryRows(query);

                    Coordinate coord = new Coordinate(latitude, longitude, searchedTime);
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
                                DynamicInfluxRow best = results.OrderBy(w => Math.Abs(Convert.ToDouble(w.Fields.First().Value) - wind)).First();

                                TimeWind compareX = new TimeWind { dateTime = (DateTime)best.Timestamp, wind = Convert.ToDouble(best.Fields.Values.First()) };
                                List<TimeWind> compareYs = await GetBestDays(devId, ws, (double)compareX.wind, ((DateTime)compareX.dateTime).TimeOfDay);

                                if (compareYs != null)
                                {
                                    if (compareYs.Count > 0)
                                    {
                                        double? diffX = await GetTemperaturesDiff(devId, (DateTime)compareX.dateTime, 0);
                                        double diffYsAvg = 0;

                                        List<double> diffYs = new List<double>();
                                        foreach (TimeWind day in compareYs)
                                        {
                                            double? diff = await GetTemperaturesDiff(devId, (DateTime)day.dateTime, WindTempCorrection(compareX.wind, day.wind));
                                            
                                            if (diff != null)
                                                diffYs.Add((double)diff);
                                        }

                                        if (diffYs.Count > 0)
                                            diffYsAvg = diffYs.Average();

                                        if (diffX != null && Math.Abs(diffYsAvg) > 0)
                                        {
                                            substitutionWeatherCoeff = (double)diffX / diffYsAvg;
                                            failed = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (failed)
                    {
                        DefaultWeatherCoeffs coeffs;

                        if (ws[0] == 800)
                            coeffs = CoeffsClear;
                        else
                            coeffs = CoeffsClouds;

                        switch (weatherId)
                        {
                            case int n when (n < 800 && n >= 700):
                                substitutionWeatherCoeff = coeffs.atmosphere;
                                break;
                            case int n when (n < 700 && n >= 600):
                                substitutionWeatherCoeff = coeffs.snow;
                                break;
                            case int n when (n < 600 && n >= 500):
                                substitutionWeatherCoeff = coeffs.rain;
                                break;
                            case int n when (n < 400 && n >= 300):
                                substitutionWeatherCoeff = coeffs.drizzle;
                                break;
                            case int n when (n < 300 && n >= 200):
                                substitutionWeatherCoeff = coeffs.storm;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (selection == null)                  // all attempts failed, try to get anything
            {
                selection = await GetBestDays(devId, 0, wind, DateTime.Now.TimeOfDay);
                substitutionWeatherCoeff = UncertainDataCoeff;
            }

            if (selection == null)
                return;                             // no comparable data exists, abort

            List<double> diffs = new List<double>();
            List<double> winds = new List<double>();

            foreach (TimeWind day in selection)
            {
                double? diff = await GetTemperaturesDiff(devId, (DateTime)day.dateTime, WindTempCorrection(wind, day.wind));

                if (diff != null)
                {
                    diffs.Add((double)diff);
                    if (day.wind != null)
                        winds.Add((double)day.wind);
                }
            }

            double diffAvg;
            if (diffs.Count > AvgDaysCount / 2)
                diffAvg = diffs.Average() * substitutionWeatherCoeff;
            else if (diffs.Count > 0)
                diffAvg = diffs.Average() * (((AvgDaysCount / 2) - diffs.Count) * 0.01 + 1) * substitutionWeatherCoeff;
            else
                return;

            if (diffAvg < MinimumDiff)
                diffAvg = MinimumDiff;

            double windAvg;
            if (winds.Count > 0)
                windAvg = winds.Average();
            else
                windAvg = 0;

            lock (coefficientsLocker)
                if (deviceDifferences.ContainsKey(devId))
                {
                    deviceDifferences[devId] = diffAvg;
                    deviceWinds[devId] = windAvg;
                    lastUpdate[devId] = DateTime.Now;
                }
                else
                {
                    deviceDifferences.Add(devId, diffAvg);
                    deviceWinds.Add(devId, windAvg);
                    lastUpdate.Add(devId, DateTime.Now);
                }

            if (DebugIsActive)
                Console.WriteLine($"8TA {measureName} dev: {devId} diff: {diffAvg:0.000} cnt: {diffs.Count} wind: {windAvg:0.00} alter: {substitutionWeatherCoeff:0.000}");
        }

        private async Task<List<TimeWind>> GetBestDays(int devId, int weatherId, double wind, TimeSpan searchedTimeOfDay)
        {
            return await GetBestDays(devId, new int[] { weatherId }, wind, searchedTimeOfDay);
        }

        private async Task<List<TimeWind>> GetBestDays(int devId, int[] weatherIds, double wind, TimeSpan searchedTimeOfDay)
        {
            if (weatherIds is null)
                return null;
            if (weatherIds.Length == 0)
                return null;

            List<DynamicInfluxRow> days = new List<DynamicInfluxRow>();

            string weatherClause = String.Empty;
            if (weatherIds[0] != 0)
            {
                weatherClause = $@"AND (""{DataManager.medianCondiName}""={weatherIds[0]}";

                for (int i = 1; i < weatherIds.Length; i++)
                {
                    weatherClause += $@" OR ""{DataManager.medianCondiName}""={weatherIds[i]}";
                }

                weatherClause += ") ";
            }

            DateTime searchedTime = DateTime.Today + searchedTimeOfDay;

            for (int i = 1; i <= BackDaysCount; i++)
            {
                string query = $@"SELECT mean(""{DataManager.meanWindName}"") AS ""{DataManager.meanWindName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measWeath}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime)} - {(SkippedDaysCount + i) * millisOfDay + MaxAge.TotalMilliseconds}ms AND time < {DataManager.TimeToInfluxTime(searchedTime)} - {(SkippedDaysCount + i) * millisOfDay - MaxAge.TotalMilliseconds}ms AND ""device""='{devId}' {weatherClause}GROUP BY time(10m) FILL(none)";

                List<DynamicInfluxRow> results = await dataMan.QueryRows(query);

                if (results is null)
                    continue;
                else if (results.Count > 0)
                    days.Add(results.OrderBy(w => Math.Abs(Convert.ToDouble(w.Fields.First().Value) - wind)).First());
            }

            List<TimeWind> bestDays = new List<TimeWind>();

            if (days.Count > 0)
            {
                foreach (DynamicInfluxRow row in days.OrderBy(w => Math.Abs(Convert.ToDouble(w.Fields.First().Value) - wind)).Take(AvgDaysCount))
                {
                    bestDays.Add(new TimeWind
                    {
                        dateTime = row.Timestamp,
                        wind = Convert.ToDouble(row.Fields.First().Value)
                    });
                }

                return bestDays;
            }
            else
                return null;
        }

        private async Task<double?> GetTemperaturesDiff(int devId, DateTime searchedTime, double measTempNegativeOffset)
        {
            string query = $@"SELECT mean(""{DataManager.meanValueName}"") AS ""{DataManager.meanValueName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{DataManager.measTmpA}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime - TimeSpan.FromMinutes(5))} AND time < {DataManager.TimeToInfluxTime(searchedTime + TimeSpan.FromMinutes(5))} AND ""device""='{devId}' FILL(linear)";

            DynamicInfluxRow row = await dataMan.QueryValue(query);

            if (row.Fields.Count > 0)
            {
                double valueAir = Convert.ToDouble(row.Fields.First().Value);

                query = $@"SELECT mean(""{DataManager.meanValueName}"") AS ""{DataManager.meanValueName}"" FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{measureName}"" WHERE time > {DataManager.TimeToInfluxTime(searchedTime - TimeSpan.FromMinutes(5))} AND time < {DataManager.TimeToInfluxTime(searchedTime + TimeSpan.FromMinutes(5))} AND ""device""='{devId}' FILL(linear)";

                row = await dataMan.QueryValue(query);

                if (row is null)
                    return null;

                if (row.Fields.Count > 0)
                {
                    double x = Convert.ToDouble(row.Fields.First().Value);                   
                    return x - measTempNegativeOffset - valueAir;
                }
            }

            return null;
        }

        private double WindTempCorrection(double? originalWind, double? newWind)
        {
            if (originalWind != null || newWind != null)
                return (double)(originalWind - newWind) * DegreesPerWindMeter;
            else
                return 0;
        }
    }
}
