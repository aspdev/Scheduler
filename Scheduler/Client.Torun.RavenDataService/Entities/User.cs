using System;
using System.Collections.Generic;

namespace Client.Torun.RavenDataService.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool ChangePassword { get; set; } = true;
        public string TemporaryPassword { get; set; }
        public string TokenToResetPassword { get; set; }
        public string TokenToResetPasswordValidFrom { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Clients { get; set; } = new List<string>();
        
    }
}
