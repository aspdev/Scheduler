using System.Collections.Generic;

namespace Client.Torun.Shared.DTOs
{
    public class DoctorDto
    {
        public string DoctorId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public List<string> Roles { get; set; }

        public string  Color { get; set; }
    }
}
