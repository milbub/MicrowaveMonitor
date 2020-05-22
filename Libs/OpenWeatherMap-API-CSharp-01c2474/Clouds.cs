using Newtonsoft.Json.Linq;

namespace OpenWeatherApi
{
    public class Clouds
    {
        public readonly double All;

        public Clouds(JToken cloudsData)
        {
            All = double.Parse(cloudsData.SelectToken("all").ToString());
        }
    }
}
