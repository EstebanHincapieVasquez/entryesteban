using entryesteban.Common.Models;
using entryesteban.Functions.Functions;
using entryesteban.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace entryesteban.Test.Test
{
    public class EntryApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEntry_Should_Return_200()
        {
            // Arrenge
            MockCloudTableEntrys mockEntrys = new MockCloudTableEntrys(new Uri("http://127.0.0.1.10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryRequest);

            // Act
            IActionResult response = await EntryApi.CreateEntry(request, mockEntrys, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateEntry_Should_Return_200()
        {
            // Arrenge
            MockCloudTableEntrys mockEntrys = new MockCloudTableEntrys(new Uri("http://127.0.0.1.10002/devstoreaccount1/reports"));
            Entry entryRequest = TestFactory.GetEntryRequest();
            Guid entryId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(entryId, entryRequest);

            // Act
            IActionResult response = await EntryApi.UpdateEntry(request, mockEntrys, entryId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
