// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace WeatherApiInteraction.WundergroundTenDayHourlyForecastDataClasses
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the JSON structure of the result returned from http://api.wunderground.com/api/
    /// </summary>
    public class Features
    {
        public int hourly10day { get; set; }
    }

    public class Response
    {
        public string version { get; set; }
        public string termsofService { get; set; }
        public Features features { get; set; }
    }

    public class FCTTIME
    {
        public string hour { get; set; }
        public string hour_padded { get; set; }
        public string min { get; set; }
        public string min_unpadded { get; set; }
        public string sec { get; set; }
        public string year { get; set; }
        public string mon { get; set; }
        public string mon_padded { get; set; }
        public string mon_abbrev { get; set; }
        public string mday { get; set; }
        public string mday_padded { get; set; }
        public string yday { get; set; }
        public string isdst { get; set; }
        public string epoch { get; set; }
        public string pretty { get; set; }
        public string civil { get; set; }
        public string month_name { get; set; }
        public string month_name_abbrev { get; set; }
        public string weekday_name { get; set; }
        public string weekday_name_night { get; set; }
        public string weekday_name_abbrev { get; set; }
        public string weekday_name_unlang { get; set; }
        public string weekday_name_night_unlang { get; set; }
        public string ampm { get; set; }
        public string tz { get; set; }
        public string age { get; set; }
        public string UTCDATE { get; set; }
    }

    public class Temp
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Dewpoint
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Wspd
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Wdir
    {
        public string dir { get; set; }
        public string degrees { get; set; }
    }

    public class Windchill
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Heatindex
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Feelslike
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Qpf
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Snow
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class Mslp
    {
        public string english { get; set; }
        public string metric { get; set; }
    }

    public class HourlyForecast
    {
        public FCTTIME FCTTIME { get; set; }
        public Temp temp { get; set; }
        public Dewpoint dewpoint { get; set; }
        public string condition { get; set; }
        public string icon { get; set; }
        public string icon_url { get; set; }
        public string fctcode { get; set; }
        public string sky { get; set; }
        public Wspd wspd { get; set; }
        public Wdir wdir { get; set; }
        public string wx { get; set; }
        public string uvi { get; set; }
        public string humidity { get; set; }
        public Windchill windchill { get; set; }
        public Heatindex heatindex { get; set; }
        public Feelslike feelslike { get; set; }
        public Qpf qpf { get; set; }
        public Snow snow { get; set; }
        public string pop { get; set; }
        public Mslp mslp { get; set; }

        public DateTime observationDateTime { get; set; }
    }

    public class RootObject
    {
        public Response response { get; set; }
        public List<HourlyForecast> hourly_forecast { get; set; }
    }

}