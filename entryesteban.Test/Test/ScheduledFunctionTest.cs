using entryesteban.Common.Models;
using entryesteban.Functions.Functions;
using entryesteban.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Xunit;

namespace entryesteban.Test.Test
{
    public class ScheduledFunctionTest
    {
        [Fact]
        public async void ScheduledFunction_Should_Log_Message()
        {
            // Arrenge
            MockCloudTableEntrys mockEntrys = new MockCloudTableEntrys(new Uri("http://127.0.0.1.10002/devstoreaccount1/reports"));
            MockCloudTableConsolidates mockConsolidates = new MockCloudTableConsolidates(new Uri("http://127.0.0.1.10002/devstoreaccount1/reports"));
            
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            // Act
            ScheduledFunction.Run(null, mockEntrys, mockConsolidates, logger);
            string message = logger.Logs[0];

            // Assert
            Assert.Contains("function processed", message);
        }
    }
}
