﻿using System;

namespace entryesteban.Common.Models
{
    public class Entry
    {
        public int IDEmployee { get; set; }
        public DateTime DateTime { get; set; }
        public int Type { get; set; }  
        public bool Consolidate { get; set; }
    }
}
