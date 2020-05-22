using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OpenWeatherApi
{
    public class Query
    {
        private readonly bool validRequest;
        private readonly Coord coord;
        private readonly List<Weather> weathers = new List<Weather>();
        private readonly string baseStr;
        private readonly Main main;
        private readonly double visibility;
        private readonly Wind wind;
        private readonly Rain rain;
        private readonly Snow snow;
        private readonly Clouds clouds;
        private readonly Sys sys;
        private readonly int id;
        private readonly string name;
        private readonly int cod;

        public bool ValidRequest { get => validRequest; }
        public Coord Coord { get => coord; }
        public List<Weather> Weathers { get => weathers; }
        public string Base { get => baseStr; }
        public Main Main { get => main; }
        public double Visibility { get => visibility; }
        public Wind Wind { get => wind; }
        public Rain Rain { get => rain; }
        public Snow Snow { get => snow; }
        public Clouds Clouds { get => clouds; }
        public Sys Sys { get => sys; }
        public int ID { get => id; }
        public string Name { get => name; }
        public int Cod { get => cod; }

        public Query(string apiKey, string latitude, string longitude)
        {
            JObject jsonData = JObject.Parse(new System.Net.WebClient().DownloadString(string.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units=metric&appid={2}", latitude, longitude, apiKey)));

            if (jsonData.SelectToken("cod").ToString() == "200")
            {
                validRequest = true;
                coord = new Coord(jsonData.SelectToken("coord"));
                foreach (JToken weather in jsonData.SelectToken("weather"))
                    weathers.Add(new Weather(weather));
                baseStr = jsonData.SelectToken("base").ToString();
                main = new Main(jsonData.SelectToken("main"));
                if (jsonData.SelectToken("visibility") != null)
                    visibility = double.Parse(jsonData.SelectToken("visibility").ToString());
                wind = new Wind(jsonData.SelectToken("wind"));
                if (jsonData.SelectToken("rain") != null)
                    rain = new Rain(jsonData.SelectToken("rain"));
                if (jsonData.SelectToken("snow") != null)
                    snow = new Snow(jsonData.SelectToken("snow"));
                clouds = new Clouds(jsonData.SelectToken("clouds"));
                sys = new Sys(jsonData.SelectToken("sys"));
                id = int.Parse(jsonData.SelectToken("id").ToString());
                name = jsonData.SelectToken("name").ToString();
                cod = int.Parse(jsonData.SelectToken("cod").ToString());
            }
            else
            {
                validRequest = false;
            }
        }
    }
}
