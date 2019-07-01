using System;
using Client.Torun.DataService.Enums;

namespace Client.Torun.DataService.Entities
{
    public class DayOffRequest
    {
        public Guid DayOffRequestId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public RequestStatus RequestStatus { get; set; }
    }
}