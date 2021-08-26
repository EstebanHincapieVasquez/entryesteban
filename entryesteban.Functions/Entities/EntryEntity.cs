using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace entryesteban.Functions.Entities
{
    public class EntryEntity : TableEntity
    {
        public int IDEmpleado { get; set; }
        public DateTime DateTime { get; set; }
        public Boolean Type { get; set; }
        public bool Consolidado { get; set; }

    }
}
