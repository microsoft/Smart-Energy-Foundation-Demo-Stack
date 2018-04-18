// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
namespace DataApi.Controllers
{
    using Swashbuckle.Swagger.Annotations;
    using Microsoft.Ajax.Utilities;
    using Microsoft.Azure;

    using SmartEnergyAzureDataTypes;

    using SmartEnergyOM;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Http;

    /// <summary>
    /// Controller to retrieve Grid Emissions Relative Merit values
    /// </summary>
    public class GridEmissionsRelativeMeritController : ApiController
    {
        // GET api/GridEmissionsRelativeMerit
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            try
            {
                var databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    var result = _objectModel.FindAllEmissionsRegions();
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Sorry - an exception occured executing the request");
            }
        }

        // GET api/GridEmissionsRelativeMerit/US_PJM
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([FromUri] string id, [FromUri] DateTime? startDateTime = null, [FromUri] DateTime? endDateTime = null,
            [FromUri] double dateTimeFlexabilityInMinutes = 0)
        {
            try
            {
                // Set default start and end datetimes
                string emissionsRegionFriendlyName = id.IsNullOrWhiteSpace() ? "US_PJM" : id;
                var startDateTimeProcessed = startDateTime ?? DateTime.UtcNow.AddHours(-3);
                var endDateTimeProcessed = endDateTime ?? DateTime.UtcNow.AddHours(1);

                // Query database
                var databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    var regionId = _objectModel.FindEmissionsRegion(emissionsRegionFriendlyName).EmissionsRegionID;

                    var result = _objectModel.FindCarbonEmissionsRelativeMeritDataPoints(regionId, startDateTimeProcessed, endDateTimeProcessed, dateTimeFlexabilityInMinutes).ToList();
                    
                    // Convert the final list to objects of the common data type in the SmartEnergyAzureDataTypes NuGet package
                    var webResult = this.ConvertToCarbonEmissionsRelativeMeritWebDataPoints(result);

                    if (result == null)
                    {
                        return this.NotFound();
                    }

                    return Ok(webResult);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Sorry - an exception occured executing the request");
            }
        }

        /// <summary>
        /// Convert from the SmartEnergyDatabase type to the SmartEnergyAzureDataTypes.EmissionsRelativeMeritDatapoint data type in the SmartEnergyAzureDataTypes NuGet package
        /// </summary>
        /// <param name="carbonEmissionsRelativeMeritDataPoint"></param>
        /// <returns></returns>
        public EmissionsRelativeMeritDatapoint ConvertToCarbonEmissionsRelativeMeritWebDataPoints(CarbonEmissionsRelativeMeritDataPoint carbonEmissionsRelativeMeritDataPoint)
        {
            EmissionsRelativeMeritDatapoint webDataPoint = new EmissionsRelativeMeritDatapoint
            {
                EmissionsRegionID = carbonEmissionsRelativeMeritDataPoint.EmissionsRegionID,
                DateTimeUTC = carbonEmissionsRelativeMeritDataPoint.DateTimeUTC,
                EmissionsRelativeMerit = carbonEmissionsRelativeMeritDataPoint.EmissionsRelativeMerit,
                EmissionsRelativeMerit_Forcast = carbonEmissionsRelativeMeritDataPoint.EmissionsRelativeMerit_Forcast
            };

            return webDataPoint;
        }
        /// <summary>
        /// Convert from the SmartEnergyDatabase type to the SmartEnergyAzureDataTypes.EmissionsRelativeMeritDatapoint data type in the SmartEnergyAzureDataTypes NuGet package
        /// </summary>
        /// <param name="carbonEmissionsDataPoints"></param>
        /// <returns></returns>
        public List<EmissionsRelativeMeritDatapoint> ConvertToCarbonEmissionsRelativeMeritWebDataPoints(
            List<CarbonEmissionsRelativeMeritDataPoint> carbonEmissionsDataPoints)
        {
            return carbonEmissionsDataPoints?.Select(this.ConvertToCarbonEmissionsRelativeMeritWebDataPoints).ToList();
        }
    }
}