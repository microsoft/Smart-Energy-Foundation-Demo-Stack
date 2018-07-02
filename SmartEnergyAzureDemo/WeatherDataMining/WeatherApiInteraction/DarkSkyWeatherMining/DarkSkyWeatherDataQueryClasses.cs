namespace WeatherApiInteraction.DarkSkyWeatherMining
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DarkSkyWeatherResultsList
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timezone { get; set; }
        public Hourly hourly { get; set; }
        public Daily daily { get; set; }
        public Flags flags { get; set; }
        public int offset { get; set; }
    }

    public class HourlyDatum
    {
        public int time { get; set; }
        public DateTime dateTime { get; set; }

        public string summary { get; set; }
        public string icon { get; set; }
        public string precipType { get; set; }
        public double temperature { get; set; }
        public double apparentTemperature { get; set; }
        public double dewPoint { get; set; }
        public double humidity { get; set; }
        public double windSpeed { get; set; }
        public double windBearing { get; set; }
        public double visibility { get; set; }
        public double? pressure { get; set; }
        public double? cloudCover { get; set; }
    }

    public class Hourly
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public IList<HourlyDatum> data { get; set; }
    }

    public class Datum
    {
        public int time { get; set; }
        public DateTime dateTime { get; set; }

        public string summary { get; set; }
        public string icon { get; set; }

        public int sunriseTime { get; set; }
        public DateTime sunriseTimeDateTime { get; set; }

        public int sunsetTime { get; set; }
        public DateTime sunsetTimeDateTime { get; set; }

        public double moonPhase { get; set; }
        public double precipAccumulation { get; set; }
        public string precipType { get; set; }
        public double temperatureHigh { get; set; }

        public double temperatureHighTime { get; set; }
        public DateTime temperatureHighTimeDateTime { get; set; }

        public double temperatureLow { get; set; }

        public double temperatureLowTime { get; set; }
        public DateTime temperatureLowTimeDateTime { get; set; }

        public double apparentTemperatureHigh { get; set; }

        public double apparentTemperatureHighTime { get; set; }
        public DateTime apparentTemperatureHighTimeDateTime { get; set; }

        public double apparentTemperatureLow { get; set; }

        public double apparentTemperatureLowTime { get; set; }
        public DateTime apparentTemperatureLowTimeDateTime { get; set; }

        public double dewPoint { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public double windSpeed { get; set; }
        public double windBearing { get; set; }
        public double cloudCover { get; set; }
        public double visibility { get; set; }
        public double temperatureMin { get; set; }

        public double temperatureMinTime { get; set; }
        public DateTime temperatureMinTimeDateTime { get; set; }

        public double temperatureMax { get; set; }

        public double temperatureMaxTime { get; set; }
        public DateTime temperatureMaxTimeDateTime { get; set; }

        public double apparentTemperatureMin { get; set; }

        public double apparentTemperatureMinTime { get; set; }
        public DateTime apparentTemperatureMinTimeDateTime { get; set; }

        public double apparentTemperatureMax { get; set; }

        public double apparentTemperatureMaxTime { get; set; }
        public DateTime apparentTemperatureMaxTimeDateTime { get; set; }
    }

    public class Daily
    {
        public IList<Datum> data { get; set; }
    }

    public class Flags
    {
        public IList<string> sources { get; set; }
        [JsonProperty("isd-stations")]
        public IList<string> isdstations { get; set; }
        public string units { get; set; }
    }
}



