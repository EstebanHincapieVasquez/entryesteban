using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace entryesteban.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int IDEmployee { get; set; }
        public DateTime DateTime { get; set; }
        public int MinutesWork { get; set; }
    }
}
