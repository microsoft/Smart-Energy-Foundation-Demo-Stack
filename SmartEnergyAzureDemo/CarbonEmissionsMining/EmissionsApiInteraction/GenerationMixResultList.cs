// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissionsApiInteraction
{
    /// <summary>
    /// Represents the JSON structure of the result returned from https://api.watttime.org/api/v1/datapoints/ 
    /// </summary>
    public class GenerationMixResultList
    {
        public class Genmix
        {
            public string fuel { get; set; }

            public double? gen_MW { get; set; }
        }

        public class Result
        {
            public DateTime timestamp { get; set; }

            public string created_at { get; set; }

            public double? carbon { get; set; }

            public List<Genmix> genmix { get; set; }

            public string url { get; set; }

            public string market { get; set; }

            public string freq { get; set; }

            public string ba { get; set; }
        }

        public class RootObject
        {
            public int count { get; set; }

            public string next { get; set; }

            public object previous { get; set; }

            public List<Result> results { get; set; }
        }

    }
}