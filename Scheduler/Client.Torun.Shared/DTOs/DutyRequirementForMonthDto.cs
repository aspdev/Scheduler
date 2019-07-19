using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Torun.Shared.DTOs
{
    public class DutyRequirementForMonthDto
    {
        public string UserId { get; set; }
        public int RequiredTotalDutiesInMonth { get; set; }
        public int RequiredTotalWeekdayDuties { get; set; }
        public int RequiredTotalHolidayDuties { get; set; }

    }
}
