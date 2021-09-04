﻿using entryesteban.Common.Models;
using entryesteban.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace entryesteban.Test.Helpers
{
    public class TestFactory
    {
        public static EntryEntity GetEntryEntity()
        {
            return new EntryEntity
            {
                ETag = "*",
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                IDEmployee = 1,
                DateTime = DateTime.UtcNow,
                Type = 0,
                Consolidate = false
            };
        }

        //Update element
        public static DefaultHttpRequest CreateHttpRequest(Guid entryId, Entry entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{entryId}"
            };
        }

        //Get by Id or Delet by Id
        public static DefaultHttpRequest CreateHttpRequest(Guid entryId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{entryId}"
            };
        }

        //Create (add new record)
        public static DefaultHttpRequest CreateHttpRequest(Entry entryRequest)
        {
            string request = JsonConvert.SerializeObject(entryRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        //Get all (return all items)
        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Entry GetEntryRequest()
        {
            return new Entry
            {
                IDEmployee = 1,
                DateTime = DateTime.UtcNow,
                Type = 0,
                Consolidate = false,
            };
        }

        private static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }

        public static ConsolidatedEntity GetConsolidatedEntity()
        {
            return new ConsolidatedEntity
            {
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                IDEmployee = 1,
                DateTime = DateTime.Now,
                MinutesWork = 540,
            };
        }

        public static Consolidated GetConsolidatedRequest()
        {
            return new Consolidated
            {
                IDEmployee = 1,
                DateTime = DateTime.UtcNow,
                MinutesWork = 540,
            };
        }

        //Update element
        public static DefaultHttpRequest CreateHttpRequestConsolidated(Guid consolidatedId, Consolidated ConsolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(ConsolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{consolidatedId}"
            };
        }

        //Get by Id or Delet by Id
        public static DefaultHttpRequest CreateHttpRequestConsolidated(Guid consolidatedId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{consolidatedId}"
            };
        }

        //Create (add new record)
        public static DefaultHttpRequest CreateHttpRequestConsolidated(Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        //Get all (return all items)
        public static DefaultHttpRequest CreateHttpRequestConsolidated()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

    }
}
