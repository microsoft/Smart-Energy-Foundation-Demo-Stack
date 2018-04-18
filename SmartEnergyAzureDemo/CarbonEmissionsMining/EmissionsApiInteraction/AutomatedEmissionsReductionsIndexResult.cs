using Newtonsoft.Json;
using System;

namespace EmissionsApiInteraction
{
    /// <summary>
    /// Represents the JSON structure of the result returned from  https://api2.watttime.org/v2/index
    /// </summary>
    public class AutomatedEmissionsReductionsIndexResult
    {
        public string ba { get; set; }
        public string freq { get; set; }
        public int rating { get; set; }
        public double percent { get; set; }

        [JsonProperty("switch")]
        public int switchValue { get; set; }
        public string market { get; set; }
        public DateTime validUntil { get; set; }
        public string moerWentUp { get; set; }
        public int validFor { get; set; }
    }
}


