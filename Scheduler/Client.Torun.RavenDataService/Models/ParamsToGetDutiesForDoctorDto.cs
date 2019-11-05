using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Models
{
    public class ParamsToGetDutiesForDoctorDto
    {
        public string DoctorId { get; set; }
        public string Date { get; set; }
    }
}
