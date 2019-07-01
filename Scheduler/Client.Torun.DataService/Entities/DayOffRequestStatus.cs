using Client.Torun.DataService.Enums;

namespace Client.Torun.DataService.Entities
{
    public class DayOffRequestStatus
    {
        public int RequestStatusId { get; set; }
        public RequestStatus RequestStatus { get; set; }
    }
}