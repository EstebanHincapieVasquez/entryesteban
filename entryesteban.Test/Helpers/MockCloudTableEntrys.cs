using entryesteban.Functions.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace entryesteban.Test.Helpers
{
    public class MockCloudTableEntrys : CloudTable
    {
        public MockCloudTableEntrys(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableEntrys(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableEntrys(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetEntryEntity()
            });
        }
        /*
        public override async Task<TableQuerySegment<EntryEntity>>
            ExecuteQuerySegmentedAsync<EntryEntity>(TableQuery<EntryEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<EntryEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetEntryEntity() }) as TableQuerySegment<EntryEntity>);
        }
        */

    }
}
