using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OpenWeatherApi
{
    public class Main
    {
        public readonly double Temperature;
        public readonly double TempMin;
        public readonly double TempMax;
        public readonly double TempFeel;
        public readonly double Pressure;
        public readonly double Humdity;
        public readonly double SeaLevelAtm;
        public readonly double GroundLevelAtm;

        public Main(JToken mainData)
        {
            Temperature = double.Parse(mainData.SelectToken("temp").ToString());
            TempMin = double.Parse(mainData.SelectToken("temp_min").ToString());
            TempMax = double.Parse(mainData.SelectToken("temp_max").ToString());
            TempFeel = double.Parse(mainData.SelectToken("feels_like").ToString());
            Pressure = double.Parse(mainData.SelectToken("pressure").ToString());
            Humdity = double.Parse(mainData.SelectToken("humidity").ToString());
            if (mainData.SelectToken("sea_level") != null)
                SeaLevelAtm = double.Parse(mainData.SelectToken("sea_level").ToString());
            if (mainData.SelectToken("grnd_level") != null)
                GroundLevelAtm = double.Parse(mainData.SelectToken("grnd_level").ToString());
        }
    }
}
