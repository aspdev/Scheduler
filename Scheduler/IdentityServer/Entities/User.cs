using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TemporaryPassword { get; set; }
        public bool ChangePassword { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Clients { get; set; }
    }
}
