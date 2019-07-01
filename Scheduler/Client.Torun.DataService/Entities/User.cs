using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.DataService.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TokenToResetPassword { get; set; }
        public DateTime? TokenToResetPasswordValidFrom { get; set; }
        public SchedulerRole Role { get; set; }
        public int RoleId { get; set; }
        public List<DayOff> DaysOff { get; set; } = new List<DayOff>();
        public List<DayOffRequest> DayOffRequests { get; set; } = new List<DayOffRequest>();
        public List<Duty> Duties { get; set; } = new List<Duty>();
        public List<DutyRequirement> DutyRequirements { get; set; } = new List<DutyRequirement>();



    }
}
