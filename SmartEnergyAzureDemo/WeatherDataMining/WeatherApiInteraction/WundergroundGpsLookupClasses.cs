using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApiInteraction.GpsLookupClasses
{
    public class Features
    {
        public int geolookup { get; set; }
    }

    public class Response
    {
        public string version { get; set; }
        public string termsofService { get; set; }
        public Features features { get; set; }
    }

    public class Station
    {
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string icao { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class Airport
    {
        public List<Station> station { get; set; }
    }

    public class Station2
    {
        public string neighborhood { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string id { get; set; }
        public int distance_km { get; set; }
        public int distance_mi { get; set; }
    }

    public class Pws
    {
        public List<Station2> station { get; set; }
    }

    public class NearbyWeatherStations
    {
        public Airport airport { get; set; }
        public Pws pws { get; set; }
    }

    public class Location
    {
        public string type { get; set; }
        public string country { get; set; }
        public string country_iso3166 { get; set; }
        public string country_name { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string tz_short { get; set; }
        public string tz_long { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string zip { get; set; }
        public string magic { get; set; }
        public string wmo { get; set; }
        public string l { get; set; }
        public string requesturl { get; set; }
        public string wuiurl { get; set; }
        public NearbyWeatherStations nearby_weather_stations { get; set; }
    }

    public class RootObject
    {
        public Response response { get; set; }
        public Location location { get; set; }
    }
}
