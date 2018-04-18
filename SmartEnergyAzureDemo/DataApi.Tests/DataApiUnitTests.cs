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
        public void TestEmissionsControllerGetAll()
        {
            // Arrange
            EmissionsController controller = new EmissionsController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void TestEmissionsControllerGetById()
        {
            // Arrange
            EmissionsController controller = new EmissionsController();
            var region = "US_PJM";

            // Act
            var result = controller.Get(region);
            var resultContent = (result as OkNegotiatedContentResult<List<GridEmissionsDataPoint>>).Content;
            
            // Assert
            Assert.IsTrue(resultContent.Any());
        }

        [TestMethod]
        public void TestRegionsControllerGetAll()
        {
            // Arrange
            RegionsController controller = new RegionsController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void TestGridEmissionsRelativeMeritControllerGetById()
        {
            // Arrange
            GridEmissionsRelativeMeritController controller = new GridEmissionsRelativeMeritController();
            var region = "US_PJM";

            // Act
            var result = controller.Get(region);
            var resultContent = (result as OkNegotiatedContentResult<List<EmissionsRelativeMeritDatapoint>>).Content;

            // Assert
            Assert.IsTrue(resultContent.Any());
        }
    }
}
