using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Quickstart.Account
{
    public class SetNewPasswordInputModel
    {
        [MinLength(8, ErrorMessage = "Password must have at least 8 characters.")]
        [Required]
        [Display(Name="New Password")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        [Display(Name="Confirm New Password")]
        public string ConfirmNewPassword { get; set; }

        public string Username { get; set; }
    }
}