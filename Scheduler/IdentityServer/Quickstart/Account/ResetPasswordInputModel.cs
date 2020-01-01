using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Quickstart.Account
{
    public class ResetPasswordInputModel
    {
        [Required]
        public string Email { get; set; }
    }
}