using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.DataService.Entities
{
    public class Duty
    {
        public Guid DutyId  { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }

    }
}
