using Newtonsoft.Json.Linq;
using System;

namespace OpenWeatherApi
{
    public class Sys
    {
        public readonly int Type;
        public readonly int ID;
        public readonly double Message;
        public readonly string Country;
        public readonly DateTime Sunrise;
        public readonly DateTime Sunset;

        public Sys(JToken sysData)
        {
            if (sysData.SelectToken("type") != null)
                Type = int.Parse(sysData.SelectToken("type").ToString());
            if (sysData.SelectToken("id") != null)
                ID = int.Parse(sysData.SelectToken("id").ToString());
            if (sysData.SelectToken("message") != null)
                Message = double.Parse(sysData.SelectToken("message").ToString());
            if (sysData.SelectToken("country") != null)
                Country = sysData.SelectToken("country").ToString();
            if (sysData.SelectToken("sunrise") != null)
                Sunrise = ConvertUnixToDateTime(double.Parse(sysData.SelectToken("sunrise").ToString()));
            if (sysData.SelectToken("sunset") != null)
                Sunset = ConvertUnixToDateTime(double.Parse(sysData.SelectToken("sunset").ToString()));
        }

        private DateTime ConvertUnixToDateTime(double unixTime)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(unixTime).ToLocalTime();
        }
    }
}
