using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherApi
{
    public class OpenWeather
    {
        private string openWeatherApiKey;

        public OpenWeather(string apiKey)
        {
            openWeatherApiKey = apiKey;
        }

        public void UpdateApiKey(string apiKey)
        {
            openWeatherApiKey = apiKey;
        }

        public Query Query(string latitude, string longitude)
        {
            Query newQuery = new Query(openWeatherApiKey, latitude, longitude);
            if (newQuery.ValidRequest)
                return newQuery;
            return null;
        }
    }
}
