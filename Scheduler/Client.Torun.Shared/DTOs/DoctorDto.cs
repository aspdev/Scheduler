using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Torun.Shared.DTOs
{
    public class DoctorDto
    {
        public string DoctorId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get; set; }
    }
}
