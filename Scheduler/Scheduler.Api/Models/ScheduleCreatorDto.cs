using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Api.Models
{
    public class ScheduleCreatorDto
    {
        
        public string AssemblyDirName { get; set; }
        public DateTime? Date { get; set; }
        public List<Object> Args { get; set; }
    }
}
