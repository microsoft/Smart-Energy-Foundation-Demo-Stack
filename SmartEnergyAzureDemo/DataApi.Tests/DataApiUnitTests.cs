// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataApi.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Results;

    using DataApi.Controllers;

    using Newtonsoft.Json;

    using SmartEnergyAzureDataTypes;

    using SmartEnergyOM;

    [TestClass]
    public class DataApiUnitTests
    {
        [TestMethod]
        public void TestGetAll()
        {
            // Arrange
            EmissionsController emissionsController = new EmissionsController();

            // Act
            var result = emissionsController.Get();

            // Assert
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void TestGetById()
        {
            // Arrange
            EmissionsController emissionsController = new EmissionsController();
            var region = "US_PJM";

            // Act
            var result = emissionsController.Get(region);
            var resultContent = (result as OkNegotiatedContentResult<List<GridEmissionsDataPoint>>).Content;
            
            // Assert
            Assert.IsTrue(resultContent.Any());
        }
    }
}
