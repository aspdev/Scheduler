using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Quickstart.Account
{
    public class ChangePasswordViewModel
    {
        
        public string Username { get; set; }
       
        public string ReturnUrl { get; set; }

        [MinLength(8, ErrorMessage = "Password must have at least 8 characters.")]
        [Required(ErrorMessage = "This filed is required")]
        [Display(Name="New Password")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [Compare("NewPassword", ErrorMessage = "New password and Confirm password do not match.")]
        [Display(Name="Confirm New Password")]
        public string ConfirmNewPassword { get; set; }



    }
}
