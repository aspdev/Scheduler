using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Torun.RavenDataService.Models
{
    public class IsRequirementSetParamsDto
    {
        [Required]
        public DateTime? Date { get; set; }

        [Required]
        public string UserId { get; set; }

    }
}
