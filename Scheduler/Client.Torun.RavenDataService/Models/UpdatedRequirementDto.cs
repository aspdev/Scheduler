namespace Client.Torun.RavenDataService.Models
{
    public class UpdatedRequirementDto
    {
        public string Id { get; set; }
        public int RequiredTotalDutiesInMonth { get; set; }
        public int TotalHolidayDuties { get; set; }
        public int TotalWeekdayDuties { get; set; }
    }
}