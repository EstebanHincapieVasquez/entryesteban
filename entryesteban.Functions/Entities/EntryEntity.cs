﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace entryesteban.Functions.Entities
{
    public class EntryEntity : TableEntity
    {
        public int IDEmployee { get; set; }
        public DateTime DateTime { get; set; }
        public int Type { get; set; }
        public bool Consolidate { get; set; }

    }
}
