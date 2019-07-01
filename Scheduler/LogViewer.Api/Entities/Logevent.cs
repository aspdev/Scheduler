using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace LogViewer.Api.Entities
{
    public class Logevent
    {
        [Date(Name = "@timestamp")]
        public DateTime Timestamp { get; set; }
                
        public string Level { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
        public string Appbasepath { get; set; }
    }
}
