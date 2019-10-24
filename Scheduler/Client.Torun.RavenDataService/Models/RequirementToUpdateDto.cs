using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Models
{
    public class RequirementToUpdateDto
    {
        [Required]
        public string Id { get; set; }
        public int RequiredTotalDutiesInMonth { get; set; }
        public int TotalHolidayDuties { get; set; }
        public int TotalWeekdayDuties { get; set; }
    }
}
