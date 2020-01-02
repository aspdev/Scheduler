using System;
using System.Threading.Tasks;
using Common;
using IdentityServer.DataStore;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Scheduler.Mailer.Interfaces;

namespace IdentityServer.Quickstart.Account
{
    [Route("account/reset")]
    public class ResetPasswordController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ISchedulerMailer _schedulerMailer;
        private readonly IdentityServerConfiguration _configuration;
        private readonly IDocumentStore _store;

        public ResetPasswordController(IUserRepository userRepository, 
            IDocumentStoreHolder documentStoreHolder, 
            ISchedulerMailer schedulerMailer,
            IdentityServerConfiguration configuration)
        {
            _userRepository = userRepository;
            _schedulerMailer = schedulerMailer;
            _configuration = configuration;
            _store = documentStoreHolder.Store;
        }
        
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordInputModel inputModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.FindByUsername(inputModel.Email);

                if (user != null)
                {
                    using (var session = _store.OpenAsyncSession())
                    {
                        var tokenToResetPassword = Convert.ToBase64String(PasswordHasher.GenerateSalt());
                        var tokenToResetPasswordValidUntil = DateTime.Now.AddHours(1).ToString("s");
                        user.TokenToResetPassword = tokenToResetPassword;
                        user.TokenToResetPasswordValidUntil = tokenToResetPasswordValidUntil;

                        await session.StoreAsync(user);
                        await session.SaveChangesAsync();

                        var link =
                            $"https://identityserver.arantasar.hostingasap.pl/account/set-new-password?token={tokenToResetPassword}";
                        var message = $"<b>Dear {user.FirstName}</b></br><p>You're receiving this message because you requested resetting your password in Scheduler Account</p><p>Follow the link below to set a new password:</p><p><a href={link}>Reset Password</a></p><p>The link will be valid for 1 hour</p><p>If you didn't request resetting your credentials, ignore this message.</p><p>Best</p><p>Scheduler Team</p>";

                        _schedulerMailer.SendMail(_configuration.MailService.FromName,
                            user.Email, "Reset Password", message, _configuration.MailService.MailBoxPassword);
                    }
                }
            }
            
            return View("~/Views/Account/ResetPassword.cshtml", inputModel);
        }
        
    }
}