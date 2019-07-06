using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Models
{
    public class UserToCreateDto
    {
        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }
        [EmailAddress]
        [Required]
        [MaxLength(30)]
        public string Email { get; set; }
        
        
    }
}
