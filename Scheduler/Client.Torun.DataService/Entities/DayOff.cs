using System;

namespace Client.Torun.DataService.Entities
{
    public class DayOff
    {
        public Guid DayOffId { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
    }
}