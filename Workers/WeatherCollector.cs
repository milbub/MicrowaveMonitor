using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using MicrowaveMonitor.Database;
using OpenWeatherApi;

namespace MicrowaveMonitor.Workers
{
    public class WeatherCollector
    {
        public static int MinRefresh { get; } = 5;    // 20 min

        private Dictionary<int, DeviceDisplay> displays;
        private Dictionary<int, string> deviceLatitude = new Dictionary<int, string>();
        private Dictionary<int, string> deviceLongitude = new Dictionary<int, string>();

        public bool IsRunning { get; set; }
        private Thread tCollector;
        private OpenWeather weatherApi = new OpenWeather(ConfigurationManager.AppSettings.Get("WeatherApiKey"));

        public WeatherCollector(Dictionary<int, DeviceDisplay> deviceDisplays)
        {
            displays = deviceDisplays;
        }

        public void AddDevice(int deviceId, string latitude, string longitude)
        {
            deviceLatitude.Add(deviceId, latitude);
            deviceLongitude.Add(deviceId, longitude);
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
                        DateTime start = DateTime.Now;
                        TimeSpan refresh = new TimeSpan(0, MinRefresh, 0);
                        TimeSpan apiWaitTime = TimeSpan.FromSeconds(1.1);

                        foreach (int devId in deviceLatitude.Keys)
                        {
                            Query query = weatherApi.Query(deviceLatitude[devId], deviceLongitude[devId]);
                            displays[devId].WeatherIcon = query.Weathers[0].Icon;
                            displays[devId].WeatherTemp = query.Main.Temperature;
                            displays[devId].WeatherWind = query.Wind.SpeedMetersPerSecond;
                            if (query.Rain != null)
                                displays[devId].WeatherRain = query.Rain.H3;
                            if (query.Snow != null)
                                displays[devId].WeatherSnow = query.Snow.H3;

                            TimeSpan diffIter = DateTime.Now - start;
                            if (diffIter < apiWaitTime)
                                Thread.Sleep(apiWaitTime - diffIter);
                        }

                        TimeSpan diffCycle = DateTime.Now - start;
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
        }
    }
}
