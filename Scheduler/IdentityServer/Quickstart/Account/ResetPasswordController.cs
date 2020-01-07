using System;
using System.Threading.Tasks;
using System.Web;
using Common;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Mailer.Interfaces;

namespace IdentityServer.Quickstart.Account
{
    [Route("account")]
    public class ResetPasswordController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ISchedulerMailer _schedulerMailer;
        private readonly IdentityServerConfiguration _configuration;

        public ResetPasswordController(IUserRepository userRepository, 
            ISchedulerMailer schedulerMailer,
            IdentityServerConfiguration configuration)
        {
            _userRepository = userRepository;
            _schedulerMailer = schedulerMailer;
            _configuration = configuration;
        }
        
        [HttpGet]
        [Route("reset")]
        public IActionResult ResetPassword()
        {
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        [HttpPost]
        [Route("reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.FindByUsername(inputModel.Email);

                if (user != null)
                {

                    if (user.ChangePassword)
                    {
                        return View("~/Views/Account/SentEmailToResetPasswordInfo.cshtml");
                    }
                    
                    var tokenToResetPassword = Convert.ToBase64String(PasswordHasher.GenerateSalt());
                    var tokenToResetPasswordValidUntil = DateTime.Now.AddHours(1).ToString("s");
                    user.TokenToResetPassword = tokenToResetPassword;
                    user.TokenToResetPasswordValidUntil = tokenToResetPasswordValidUntil;

                    await _userRepository.UpdateUser(user);

                    var urlEncodedToken = HttpUtility.UrlEncode(tokenToResetPassword);
                    var urlEncodedUserName = HttpUtility.UrlEncode(user.Email);
                    
                    var link =
                        $"https://identityserver.arantasar.hostingasp.pl/account/set-new-password?token={urlEncodedToken}&username={urlEncodedUserName}";
                    var message = $"<b>Dear {user.FirstName}</b></br><p>You're receiving this message because you requested resetting your password in Scheduler Account</p><p>Follow the link below to set a new password:</p><p><a href={link}>Reset Password</a></p><p>The link will be valid for 1 hour</p><p>If you didn't request resetting your credentials, ignore this message.</p><p>Best</p><p>Scheduler Team</p>";

                    _schedulerMailer.SendMail(_configuration.MailService.FromName,
                        user.Email, "Reset Password", message, _configuration.MailService.MailBoxPassword);
                }
                
                return View("~/Views/Account/SentEmailToResetPasswordInfo.cshtml");
            }
            
            return View("~/Views/Account/ResetPassword.cshtml", inputModel);
        }

        [HttpGet]
        [Route("set-new-password")]
        public async Task<IActionResult> SetNewPassword([FromQuery] string token, string username)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
            {
                return BadRequest();
            }

            var user = await _userRepository.FindByUsername(username);

            if (user is null)
            {
                return BadRequest();
            }
            
            
            var tokenValidUntil = DateTime.Parse(user.TokenToResetPasswordValidUntil);
            
            if (user.TokenToResetPassword.Equals(token) == false ||
                tokenValidUntil < DateTime.Now)
            {
                return BadRequest();
            }

            var vm = new SetNewPasswordInputModel
            {
                Username = username
            };
            
            return View("~/Views/Account/FormToResetPassword.cshtml", vm);
        }

        [HttpPost]
        [Route("set-new-password")]
        public async Task<IActionResult> SetNewPassword(SetNewPasswordInputModel setNewPasswordInput)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.FindByUsername(setNewPasswordInput.Username);

                var salt = Convert.FromBase64String(user.Salt);
                var newPassword = PasswordHasher.HashPassword(setNewPasswordInput.NewPassword, salt);
                user.Password = newPassword;
                user.TokenToResetPassword = null;
                await _userRepository.UpdateUser(user);

                return View("~/Views/Account/ResetPasswordSuccess.cshtml");
            }

            return View("~/Views/Account/FormToResetPassword.cshtml", setNewPasswordInput);
        }
    }
}