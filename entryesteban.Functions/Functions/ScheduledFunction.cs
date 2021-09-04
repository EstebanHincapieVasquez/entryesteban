using entryesteban.Common.Responses;
using entryesteban.Functions.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace entryesteban.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
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
                        DateTime = DateTime.Now,
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
            log.LogInformation($"New consolidations: {contNew} stored in table successfully and {contUpdate} consolidations updates.");
        }
    }
}
