using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Models
{
    public class RequirementToSetDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime? Date { get; set; }
        public int RequiredTotalDutiesInMonth { get; set; }
        public int TotalHolidayDuties { get; set; }
        public int TotalWeekdayDuties { get; set; }
    }
}
