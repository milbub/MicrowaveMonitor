﻿using Newtonsoft.Json.Linq;

namespace OpenWeatherApi
{
    public class Coord
    {
        public readonly double Longitude;
        public readonly double Latitude;

        public Coord(JToken coordData)
        {
            Longitude = double.Parse(coordData.SelectToken("lon").ToString());
            Latitude = double.Parse(coordData.SelectToken("lat").ToString());
        }
    }
}
