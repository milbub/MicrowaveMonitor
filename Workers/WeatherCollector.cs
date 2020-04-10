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
        public static int MinRefresh { get; } = 5;          // 5 min
        public static int ApiWaitTime { get; } = 1200;      // 1200 msec

        private readonly Dictionary<int, DeviceDisplay> displays;
        private readonly Queue<DynamicInfluxRow> database;
        private readonly Dictionary<int, string> deviceLatitude = new Dictionary<int, string>();
        private readonly Dictionary<int, string> deviceLongitude = new Dictionary<int, string>();

        public bool IsRunning { get; set; }
        private Thread tCollector;
        private readonly OpenWeather weatherApi = new OpenWeather(ConfigurationManager.AppSettings.Get("WeatherApiKey"));

        public WeatherCollector(Queue<DynamicInfluxRow> dbRows, Dictionary<int, DeviceDisplay> deviceDisplays)
        {
            displays = deviceDisplays;
            database = dbRows;
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

        public void Start()
        {
            if (IsRunning == false)
            {
                IsRunning = true;
                tCollector = new Thread(() =>
                {
                    while (IsRunning)
                    {
                        DateTime startCycle = DateTime.Now;
                        TimeSpan refresh = new TimeSpan(0, MinRefresh, 0);
                        TimeSpan apiWaitTime = new TimeSpan(0, 0, 0, 0, ApiWaitTime);

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
                            catch (System.Net.WebException e)
                            {
                                Console.WriteLine(e.Message);
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
                            } catch (KeyNotFoundException)
                            {
                                Thread.Sleep(refresh);
                                continue;
                            }

                            DynamicInfluxRow row = new DynamicInfluxRow();
                            row.Timestamp = startIter.ToUniversalTime();
                            row.Fields.Add("value", temperature);
                            row.Tags.Add("device", devId.ToString());

                            lock (database)
                                database.Enqueue(row);
                            
                            TimeSpan diffIter = DateTime.Now - startIter;
                            if (diffIter < apiWaitTime)
                                Thread.Sleep(apiWaitTime - diffIter);
                        }

                        TimeSpan diffCycle = DateTime.Now - startCycle;
                        if (diffCycle < refresh)
                            Thread.Sleep(refresh - diffCycle);
                    }
                });
                tCollector.Start();
            }
        }

        public void Stop()
        {
            IsRunning = false;
                tCollector.Abort();
        }
    }
}
