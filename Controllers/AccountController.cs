using IsIoTWeb.Models;
using IsIoTWeb.Repository;
using IsIoTWeb.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IsIoTWeb.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ISinkRepository _sinkRepository;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ISinkRepository sinkRepository)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _sinkRepository = sinkRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required] string username, [Required] string password)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User appUser = await userManager.FindByNameAsync(username);
                    var sinkId = await _sinkRepository.GetIdByUser(appUser);
                    if (appUser != null && sinkId != null)
                    {
                        Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(appUser, password, false, false);
                        StaticVariables.SinkId = sinkId;
                        if (result.Succeeded)
                        {
                            return Redirect("/");
                        }
                    }
                    ModelState.AddModelError(nameof(username), "Invalid Email or Password");
                }
                catch (TimeoutException)
                {
                    ModelState.AddModelError("Network Timeout", "A timout was encountered when sending the login request. Server might be down or connection is unstable.");
                }
                catch(Exception)
                {
                    ModelState.AddModelError("Unexpected error", "An unexpected error occured. Please contact the administrator.");
                }
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await signInManager.SignOutAsync();
            }
            catch (TimeoutException)
            {
                ModelState.AddModelError("Network Timeout", "A timout was encountered when sending the logout request. Server might be down or connection is unstable.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("Unexpected error", "An unexpected error occured. Please contact the administrator.");
            }

            StaticVariables.SinkId = null;
            return RedirectToAction("Index", "Home");
        }
    }
}
