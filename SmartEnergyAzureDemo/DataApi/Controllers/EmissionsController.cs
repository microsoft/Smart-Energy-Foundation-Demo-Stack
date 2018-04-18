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

    public class EmissionsController : ApiController
    {
        // GET api/Emissions
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

        // GET api/Emissions/US_PJM
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([FromUri] string id, [FromUri] DateTime? startDateTime = null, [FromUri] DateTime? endDateTime = null,
            [FromUri] bool returnOnlyNonNullSystemWideEmissionsDataPoints = false, [FromUri] bool returnOnlyNonNullMarginalEmissionsDataPoints = false,
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

                    var result = _objectModel.FindCarbonEmissionsDataPoints(regionId, startDateTimeProcessed, endDateTimeProcessed, dateTimeFlexabilityInMinutes).ToList();

                    // Remove any relavent results as per search parameters passed in - System Wide and Marginal Emissions
                    if(returnOnlyNonNullSystemWideEmissionsDataPoints)
                    {
                        result.RemoveAll(x => x.SystemWideCO2Intensity_gCO2kWh == null);
                    }

                    if (returnOnlyNonNullMarginalEmissionsDataPoints)
                    {
                        result.RemoveAll(x => x.MarginalCO2Intensity_gCO2kWh == null);
                    }

                    // Convert the final list to objects of the common data type in the SmartEnergyAzureDataTypes NuGet package
                    var webResult = this.ConvertToCarbonEmissionsWebDataPoints(result);

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

        // POST api/Emissions
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/Emissions/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/Emissions/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }

        public GridEmissionsDataPoint ConvertCarbonEmissionsDataPointToCarbonEmissionsWebDataPoint(CarbonEmissionsDataPoint carbonEmissionsDataPoint)
        {
            GridEmissionsDataPoint webDataPoint = new GridEmissionsDataPoint
            {
                EmissionsRegionID = carbonEmissionsDataPoint.EmissionsRegionID,
                DateTimeUTC = carbonEmissionsDataPoint.DateTimeUTC,
                SystemWideCO2Intensity_gCO2kWh =
                                                              carbonEmissionsDataPoint.SystemWideCO2Intensity_gCO2kWh,
                MarginalCO2Intensity_gCO2kWh =
                                                              carbonEmissionsDataPoint.MarginalCO2Intensity_gCO2kWh,
                SystemWideCO2Intensity_Forcast_gCO2kWh =
                                                              carbonEmissionsDataPoint.SystemWideCO2Intensity_Forcast_gCO2kWh,
                MarginalCO2Intensity_Forcast_gCO2kWh =
                                                              carbonEmissionsDataPoint.MarginalCO2Intensity_Forcast_gCO2kWh
            };

            return webDataPoint;
        }

        public List<GridEmissionsDataPoint> ConvertToCarbonEmissionsWebDataPoints(
            List<CarbonEmissionsDataPoint> carbonEmissionsDataPoints)
        {
            return carbonEmissionsDataPoints?.Select(this.ConvertCarbonEmissionsDataPointToCarbonEmissionsWebDataPoint).ToList();
        }
    }
}