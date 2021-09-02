using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using entryesteban.Common.Models;
using entryesteban.Common.Responses;
using entryesteban.Functions.Entities;
using System.Globalization;

namespace entryesteban.Functions.Functions
{
    public static class EntryApi
    {
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "entry")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new entry.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);

            if (string.IsNullOrEmpty(entry?.IDEmployee.ToString()) || string.IsNullOrEmpty(entry?.Type.ToString()) || string.IsNullOrEmpty(entry?.DateTime.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a IDEmpleado, a DateTime and the Type must be: 0 = Entry or 1 = Exit."
                });
            }

            EntryEntity entryEntity = new EntryEntity
            {
                ETag = "*",
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                IDEmployee = entry.IDEmployee,
                DateTime = Convert.ToDateTime(entry.DateTime),
                Type = entry.Type,
                Consolidate = false
            };

            TableOperation addOperation = TableOperation.Insert(entryEntity);
            await entryTable.ExecuteAsync(addOperation);

            string message = "New entry stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

        [FunctionName(nameof(UpdateEntry))]
        public static async Task<IActionResult> UpdateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for entry: {id}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Entry entry = JsonConvert.DeserializeObject<Entry>(requestBody);

            // Validate entry id
            TableOperation findOperation = TableOperation.Retrieve<EntryEntity>("TIME", id);
            TableResult findResult = await entryTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Entry not found",
                });
            }

            // Update entry
            EntryEntity entryEntity = (EntryEntity)findResult.Result;

            if (!string.IsNullOrEmpty(entry?.IDEmployee.ToString()) || !string.IsNullOrEmpty(entry?.Type.ToString()) || !string.IsNullOrEmpty(entry?.DateTime.ToString()))
            {
                entryEntity.IDEmployee = entry.IDEmployee;
                entryEntity.DateTime = entry.DateTime;
                entryEntity.Type = entry.Type;
            }
            
            TableOperation addOperation = TableOperation.Replace(entryEntity);
            await entryTable.ExecuteAsync(addOperation);

            string message = $"Entry: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

        [FunctionName(nameof(GetAllEntry))]
        public static async Task<IActionResult> GetAllEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry")] HttpRequest req,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("Get all entrys received.");

            TableQuery<EntryEntity> query = new TableQuery<EntryEntity>();
            TableQuerySegment<EntryEntity> entrys = await entryTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Received all entrys";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entrys
            });
        }

        [FunctionName(nameof(GetEntryById))]
        public static IActionResult GetEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", "TIME", "{id}", Connection = "AzureWebJobsStorage")] EntryEntity entryEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get entry by id: {id}, received.");

            if (entryEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Entry not found",
                });
            }

            string message = $"Entry: {entryEntity.RowKey}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

        [FunctionName(nameof(DeleteEntry))]
        public static async Task<IActionResult> DeleteEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "entry/{id}")] HttpRequest req,
            [Table("entry", "TIME", "{id}", Connection = "AzureWebJobsStorage")] EntryEntity entryEntity,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete entry id: {id}, received.");

            if (entryEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Entry not found",
                });
            }

            await entryTable.ExecuteAsync(TableOperation.Delete(entryEntity));

            string message = $"Entry: {entryEntity.RowKey}, deleted";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = entryEntity
            });
        }

    }
}
