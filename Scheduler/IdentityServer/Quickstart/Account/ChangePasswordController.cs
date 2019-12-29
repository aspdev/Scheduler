using IdentityServer.DataStore;
using IdentityServer.Entities;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace IdentityServer.Quickstart.Account
{
    [Route("Account/Change-Password")]
    public class ChangePasswordController : Controller
    {
        IDocumentStore _store;
        public ChangePasswordController(IDocumentStoreHolder storeHolder)
        {
            _store = storeHolder.Store;
        }

        [HttpGet]
        public IActionResult ChangePassword(string returnUrl, string username)
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel
            {
                Username = username,
                ReturnUrl = returnUrl
            };
            return View("~/Views/Account/ChangePassword.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> ValidateChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using(var session = _store.OpenAsyncSession())
                {
                    var user = await session.Query<User>().FirstAsync(u => u.Email == model.Username);

                    var salt = Convert.FromBase64String(user.Salt);
                    var hashedPassword = PasswordHasher.HashPassword(model.NewPassword, salt);
                    
                    user.Password = hashedPassword;
                    user.TemporaryPassword = null;
                    user.ChangePassword = false;

                    var url = new Uri(model.ReturnUrl);
                    var redirectUrl = url.Scheme + Uri.SchemeDelimiter + url.Host + ":" + url.Port;

                    await session.SaveChangesAsync();
                          

                    return Redirect(redirectUrl);
                }
            }

            return View("~/Views/Account/ChangePassword.cshtml", model);
        }
    }
}
