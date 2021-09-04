using entryesteban.Common.Responses;
using entryesteban.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace entryesteban.Functions.Functions
{
    public static class ConsolidatedApi
    {
        [FunctionName(nameof(ConsolidatedProcess))]
        public static async Task<IActionResult> ConsolidatedProcess(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidated")] HttpRequest req,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            ILogger log)
        {
            log.LogInformation("ConsolidatedApi function processed a request.");

            string filterNoConsolidated = TableQuery.GenerateFilterConditionForBool("Consolidate", QueryComparisons.Equal, false);
            TableQuery<EntryEntity> queryNoConsolidated = new TableQuery<EntryEntity>().Where(filterNoConsolidated);
            TableQuerySegment<EntryEntity> entrysNoConsolidated = await entryTable.ExecuteQuerySegmentedAsync(queryNoConsolidated, null);

            EntryEntity entryEntity = new EntryEntity();
            ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity();

            List<EntryEntity> ListEntryEntity = new List<EntryEntity>();
            foreach (EntryEntity entrys in entrysNoConsolidated)
            {
                ListEntryEntity.Add(entrys);
            }

            ListEntryEntity = ListEntryEntity.OrderBy(x => x.IDEmployee).ThenBy(z => z.DateTime).ToList();

            int contNew = 0;
            int contUpdate = 0;
            for (int i = 0; i < ListEntryEntity.Count; i++)
            {
                if (ListEntryEntity[i].Type == 1)
                {
                    consolidatedEntity = new ConsolidatedEntity
                    {
                        ETag = "*",
                        PartitionKey = "CONSOLIDATED",
                        RowKey = Guid.NewGuid().ToString(),
                        IDEmployee = ListEntryEntity[i].IDEmployee,
                        DateTime = ListEntryEntity[i-1].DateTime,
                        MinutesWork = (int)(ListEntryEntity[i].DateTime - ListEntryEntity[i - 1].DateTime).TotalMinutes,
                    };

                    //Query to table consolidate for see if already exist the employee
                    string findEmployeeInConsolidateTable = TableQuery.CombineFilters(TableQuery.GenerateFilterConditionForInt("IDEmployee", QueryComparisons.Equal, consolidatedEntity.IDEmployee),
                        TableOperators.And, TableQuery.CombineFilters(TableQuery.GenerateFilterConditionForDate("DateTime", QueryComparisons.GreaterThanOrEqual, consolidatedEntity.DateTime.Date),
                         TableOperators.And,
                         TableQuery.GenerateFilterConditionForDate("DateTime", QueryComparisons.LessThan, consolidatedEntity.DateTime.Date.AddDays(1))));
                    TableQuery<ConsolidatedEntity> queryEmployeeInConsolidateTable = new TableQuery<ConsolidatedEntity>().Where(findEmployeeInConsolidateTable);
                    TableQuerySegment<ConsolidatedEntity> queryResult = await consolidatedTable.ExecuteQuerySegmentedAsync(queryEmployeeInConsolidateTable, null);
                    if (queryResult.Results.Count == 0)
                    {
                        //If the employee does not exist, a new record is stored.
                        TableOperation addNewConsolidation = TableOperation.Insert(consolidatedEntity);
                        await consolidatedTable.ExecuteAsync(addNewConsolidation);
                        log.LogInformation($"New Consolidation stored in table of times for employee id: {consolidatedEntity.IDEmployee} at: {DateTime.Now}");
                        contNew++;
                    }
                    else
                    {
                        //If the employee already exists, update the number of hours worked.
                        ConsolidatedEntity consolidatedEntityOriginal = queryResult.Results[0];
                        consolidatedEntityOriginal.MinutesWork = consolidatedEntityOriginal.MinutesWork + consolidatedEntity.MinutesWork;
                        TableOperation updateConsolidation = TableOperation.Replace(consolidatedEntityOriginal);
                        await consolidatedTable.ExecuteAsync(updateConsolidation);
                        log.LogInformation($"Consolidation stored in table of times for employee id: {consolidatedEntity.IDEmployee} at: {DateTime.Now}");
                        contUpdate++;
                    }

                    //update consolidate=true in entryTable type=0
                    TableOperation findConsolidatesInFalseType0 = TableOperation.Retrieve<EntryEntity>("TIME", ListEntryEntity[i - 1].RowKey);
                    TableResult findResultType0ForConsolidate = await entryTable.ExecuteAsync(findConsolidatesInFalseType0);
                    EntryEntity entryUpdateType0 = (EntryEntity)findResultType0ForConsolidate.Result;
                    entryUpdateType0.Consolidate = true;
                    TableOperation updateEntryType0InContolidateTrue = TableOperation.Replace(entryUpdateType0);
                    await entryTable.ExecuteAsync(updateEntryType0InContolidateTrue);

                    //update consolidate=true in entryTable type=1
                    TableOperation findConsolidatesInFalseType1 = TableOperation.Retrieve<EntryEntity>("TIME", ListEntryEntity[i].RowKey);
                    TableResult findResultType1ForConsolidate = await entryTable.ExecuteAsync(findConsolidatesInFalseType1);
                    EntryEntity entryUpdateType1 = (EntryEntity)findResultType1ForConsolidate.Result;
                    entryUpdateType1.Consolidate = true;
                    TableOperation updateEntryType1InContolidateTrue = TableOperation.Replace(entryUpdateType1);
                    await entryTable.ExecuteAsync(updateEntryType1InContolidateTrue);
                }
            }
            string message = $"New consolidations: {contNew} stored in table successfully and {contUpdate} consolidations updates.";
            log.LogInformation(message);
            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
            });
        }

        [FunctionName(nameof(GetConsolidationByDate))]
        public static async Task<IActionResult> GetConsolidationByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidated/{date}")] HttpRequest req,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            string date,
            ILogger log)
        {
            log.LogInformation($"Get consolidates by date: {date}, completed.");

            string query = TableQuery.CombineFilters(TableQuery.GenerateFilterConditionForDate("DateTime", QueryComparisons.GreaterThanOrEqual, Convert.ToDateTime(date).Date),
                         TableOperators.And,
                         TableQuery.GenerateFilterConditionForDate("DateTime", QueryComparisons.LessThan, Convert.ToDateTime(date).Date.AddDays(1)));
            TableQuery<ConsolidatedEntity> queryConsolidatesForDate = new TableQuery<ConsolidatedEntity>().Where(query);
            TableQuerySegment<ConsolidatedEntity> consolidateds = await consolidatedTable.ExecuteQuerySegmentedAsync(queryConsolidatesForDate, null);
            if (consolidateds == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Consolidation not found",
                });
            }

            string message = $"Consolidation: {date}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = consolidateds
            });
        }
    }
}
