using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissionsApiInteraction
{
    /// <summary>
    /// Class representing the relative emissions evaluation of a grid at a given point in time
    /// </summary>
    public class EmissionsRelativeMeritDatapoint
    {
        public int EmissionsRegionID { get; set; }

        public DateTime Timestamp { get; set; }

        public double? EmissionsRelativeMerit { get; set; }

        public double? EmissionsRelativeMerit_Forcast { get; set; }
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public EmissionsRelativeMeritDatapoint()
        {
        }

    }
}
