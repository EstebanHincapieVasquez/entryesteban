using entryesteban.Common.Models;
using entryesteban.Functions.Functions;
using entryesteban.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace entryesteban.Test.Test
{
    public class ConsolidatedApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void GetConsolidatedByDate_Should_Return_200()
        {
            // Arrenge
            MockCloudTableConsolidates mockConsolidates = new MockCloudTableConsolidates(new Uri("http://127.0.0.1.10002/devstoreaccount1/reports"));

            string dateTime = DateTime.UtcNow.ToString();

            Consolidated consolidatedRequest = TestFactory.GetConsolidatedRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestConsolidated(consolidatedRequest);

            // Act
            IActionResult response = await ConsolidatedApi.GetConsolidationByDate(request, mockConsolidates, dateTime, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
