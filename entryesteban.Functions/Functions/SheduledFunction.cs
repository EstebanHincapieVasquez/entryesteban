using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace entryesteban.Functions.Functions
{
    public static class SheduledFunction
    {
        [FunctionName("SheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */30 * * * *")]TimerInfo myTimer,
            [Table("entry", Connection = "AzureWebJobsStorage")] CloudTable entryTable,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedEntity,
            ILogger log)
        {
            log.LogInformation($"Timer trigger consolidated function executed at: {DateTime.Now}");
        }
        /*    
            string filter = TableQuery.GenerateFilterConditionForBool("IsCompleted", QueryComparisons.Equal, true);
            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>().Where(filter);
            TableQuerySegment<TodoEntity> completedTodos = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            int deleted = 0;
            foreach (TodoEntity completedTodo in completedTodos)
            {
                await todoTable.ExecuteAsync(TableOperation.Delete(completedTodo));
                deleted++;
            }
            log.LogInformation($"Deleted: {deleted} items at: {DateTime.Now}");
            
        private static async Task SetIsConsolidatedAsync(string id, CloudTable ConsolidatedTable);
        */
    }
}
