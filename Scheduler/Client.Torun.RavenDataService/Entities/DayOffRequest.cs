using Client.Torun.RavenDataService.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Entities
{
    public class DayOffRequest
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public RequestStatus RequestStatus { get; set; }
    }
}
