using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogViewer.Api.Models
{
    public class LogeventDto
    {
        public string LogeventId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
        public string Appbasepath { get; set; }

    }
}
