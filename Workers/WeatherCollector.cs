using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using MicrowaveMonitor.Database;
using Vibrant.InfluxDB.Client.Rows;
using OpenWeatherApi;

namespace MicrowaveMonitor.Workers
{
    public class WeatherCollector
    {
        public static int MinRefresh { get; } = 300000;     // 5 min
        public static int ApiWaitTime { get; } = 1200;      // 1.2 sec (1 sec limit + 0.2 sec protect interval)

        private readonly Dictionary<int, DeviceDisplay> displays;
        private readonly Queue<DynamicInfluxRow> airTempDatabase;
        private readonly Queue<DynamicInfluxRow> weatherDatabase;
        private readonly Dictionary<int, string> deviceLatitude = new Dictionary<int, string>();
        private readonly Dictionary<int, string> deviceLongitude = new Dictionary<int, string>();

        private Thread tCollector;
        private readonly OpenWeather weatherApi = new OpenWeather(ConfigurationManager.AppSettings.Get("WeatherApiKey"));

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                if (value)
                    Start();
            }
        }

        public WeatherCollector(Queue<DynamicInfluxRow> airTempDb, Queue<DynamicInfluxRow> weatherDb, Dictionary<int, DeviceDisplay> deviceDisplays)
        {
            displays = deviceDisplays;
            airTempDatabase = airTempDb;
            weatherDatabase = weatherDb;
        }

        public void AddDevice(int deviceId, string latitude, string longitude)
        {
            lock (deviceLatitude)
                deviceLatitude.Add(deviceId, latitude);
            lock (deviceLongitude)
                deviceLongitude.Add(deviceId, longitude);
        }

        public void RemoveDevice(int deviceId)
        {
            lock (deviceLatitude)
                deviceLatitude.Remove(deviceId);
            lock (deviceLongitude)
                deviceLongitude.Remove(deviceId);
        }

        private void Start()
        {
            tCollector = new Thread(() =>
            {
                while (IsRunning)
                {
                    DateTime startCycle = DateTime.Now;
                    TimeSpan refresh = TimeSpan.FromMilliseconds(MinRefresh);
                    TimeSpan apiWaitTime = TimeSpan.FromMilliseconds(ApiWaitTime);
            
                    int[] keys;
                    lock (deviceLatitude)
                        keys = deviceLatitude.Keys.ToArray();
            
                    foreach (int devId in keys)
                    {
                        DateTime startIter = DateTime.Now;
                        Query query;
                        try
                        {
                            string lat;
                            string longi;
            
                            lock (deviceLatitude)
                                lat = deviceLatitude[devId];
                            lock (deviceLongitude)
                                longi = deviceLongitude[devId];
            
                            query = weatherApi.Query(lat, longi);
                        }
                        catch (System.Net.WebException)
                        {
                            Console.WriteLine("2Connection to weather API server is not available.");
                            continue;
                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            Console.WriteLine("2Bad format of JSON weather data.");
                            continue;
                        }
                        catch (KeyNotFoundException)
                        {
                            Thread.Sleep(refresh);
                            continue;
                        }
            
                        float temperature = (float)query.Main.Temperature;
            
                        try
                        {
                            displays[devId].WeatherIcon = query.Weathers[0].Icon;
                            displays[devId].WeatherDesc = query.Weathers[0].Description;
                            displays[devId].WeatherTemp = temperature;
                            displays[devId].WeatherWind = query.Wind.SpeedMetersPerSecond;
                        }
                        catch (KeyNotFoundException)
                        {
                            Thread.Sleep(refresh);
                            continue;
                        }
            
                        DynamicInfluxRow rowTemp = new DynamicInfluxRow();
                        rowTemp.Timestamp = startIter.ToUniversalTime();
                        rowTemp.Fields.Add("value", temperature);
                        rowTemp.Tags.Add("device", devId.ToString());
            
                        lock (airTempDatabase)
                            airTempDatabase.Enqueue(rowTemp);

                        DynamicInfluxRow rowOther = new DynamicInfluxRow();
                        rowOther.Timestamp = startIter.ToUniversalTime();
                        rowOther.Fields.Add("condition", query.Weathers[0].ID);
                        rowOther.Fields.Add("wind", query.Wind.SpeedMetersPerSecond);
                        rowOther.Tags.Add("device", devId.ToString());

                        lock (weatherDatabase)
                            weatherDatabase.Enqueue(rowOther);

                        TimeSpan diffIter = DateTime.Now - startIter;
                        if (diffIter < apiWaitTime)
                            Thread.Sleep(apiWaitTime - diffIter);
                    }
            
                    TimeSpan diffCycle = DateTime.Now - startCycle;
                    if (diffCycle < refresh)
                        Thread.Sleep(refresh - diffCycle);
                }
            }){ IsBackground = true, Name = "weatherCollector" };
            tCollector.Start();
        }
    }
}
