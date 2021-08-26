using System;
using System.Collections.Generic;
using System.Text;

namespace entryesteban.Common.Responses
{
    public class Response
    {
        public bool IsSucccess { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
