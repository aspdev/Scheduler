using System;

namespace Client.Torun.DataService.Entities
{
    public class DutyRequirement
    {
        public Guid DutyRequirementId { get; set; }
        public User User { get; set; }
        public Guid UserId  { get; set; }
        public DateTime Date { get; set; }
        public int RequiredTotalDutiesInMonth { get; set; }
        public int TotalHolidayDuties { get; set; }
        public int TotalWeekdayDuties { get; set; }
        
    }
}