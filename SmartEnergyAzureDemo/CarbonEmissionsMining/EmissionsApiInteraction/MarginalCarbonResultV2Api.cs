using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissionsApiInteraction
{
    /// <summary>
    /// Represents the JSON structure of the result returned from https://api2.watttime.org/api/v2/data/
    /// </summary>
    public class MarginalCarbonResultV2Api
    {
        public DateTime point_time { get; set; }
        public double value { get; set; }
        public int frequency { get; set; }
        public object market { get; set; }
        public string fuel { get; set; }
        public string ba { get; set; }
        public string datatype { get; set; }
    }
}
