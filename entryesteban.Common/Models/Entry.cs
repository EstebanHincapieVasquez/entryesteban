using System;

namespace entryesteban.Common.Models
{
    public class Entry
    {
        public int IDEmpleado { get; set; }
        public DateTime DateTime { get; set; }
        public Boolean Type { get; set; }  
        public bool Consolidado { get; set; }
    }
}
