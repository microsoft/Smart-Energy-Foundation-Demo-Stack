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

    public class RegionsController : ApiController
    {
        // GET api/Regions
        [SwaggerOperation("GetAll")]
        public IEnumerable<EmissionsRegionDataPoint> Get()
        {
            try
            {
                var databaseConnectionString = CloudConfigurationManager.GetSetting("SQLAzureDatabaseEntityFrameworkConnectionString");
                using (var _objectModel = new SmartEnergyOM(databaseConnectionString))
                {
                    var result = _objectModel.FindAllEmissionsRegionsAsEmissionsRegionObjects();

                    var webResult = this.ConvertToEmissionsRegionDataPoints(result);

                    return webResult;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Sorry - an exception occured executing the request");
            }
        }

        public EmissionsRegionDataPoint ConvertCarbonEmissionsDataPointToCarbonEmissionsWebDataPoint(EmissionsRegion emissionsRegion)
        {
            EmissionsRegionDataPoint webDataPoint = new EmissionsRegionDataPoint
            {
                EmissionsRegionID = emissionsRegion.EmissionsRegionID,

                FriendlyName = emissionsRegion.FriendlyName,
                TimeZoneUTCRelative = emissionsRegion.TimeZoneUTCRelative,
                Latitude = emissionsRegion.Latitude,
                Longitude = emissionsRegion.Longitude,
                EmissionsRegionWattTimeSubUrl =  emissionsRegion.EmissionsRegionWattTimeSubUrl
            };

            return webDataPoint;
        }

        public List<EmissionsRegionDataPoint> ConvertToEmissionsRegionDataPoints(
            List<EmissionsRegion> emissionsRegions)
        {
            return emissionsRegions?.Select(this.ConvertCarbonEmissionsDataPointToCarbonEmissionsWebDataPoint).ToList();
        }
    }
}