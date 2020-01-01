using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Quickstart.Account
{
    [Route("Account/Reset-Password")]
    public class ResetPasswordController : Controller
    {
        private readonly IUserRepository _userRepository;

        public ResetPasswordController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                
            }

            return View("~/Views/Account/ResetPassword.cshtml", inputModel);
        }
        
    }
}