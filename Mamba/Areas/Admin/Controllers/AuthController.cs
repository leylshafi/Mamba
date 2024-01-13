using Mamba.Areas.Admin.ViewModels;
using Mamba.Models;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signinManager;
        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signinManager)
        {
            _userManager = userManager;
            _signinManager = signinManager;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.Users.FirstOrDefaultAsync(u=>u.UserName== registerVM.UserName);
            if(user is not null)
            {
                ModelState.AddModelError("UserName", "This username is belong to someone");
                return View();
            }
            user = await _userManager.Users.FirstOrDefaultAsync(u=>u.Email==registerVM.Email);
            if(user is not null)
            {
                ModelState.AddModelError("Email", "This user already exists");
                return View();
            }
            user = new AppUser()
            {
                Name = registerVM.Name,
                Surname = registerVM.Surname,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, item.Description);
                }
                return View();
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>Login(LoginVM loginVM,string? returnUrl)
        {
            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
            if(user is null)
            {
                user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);
                if(user is null)
                {
                    ModelState.AddModelError("UserNameOrEmail", "Username, Email or Password is incorrect");
                    return View();
                }
            }

            var result = await _signinManager.PasswordSignInAsync(user, loginVM.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty, "Username, Email or Password is incorrect");
                return View();
            }
            if (returnUrl is null)
            {
                return Redirect("https://localhost:7250/");
            }
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout(string returnUrl)
        {
            await _signinManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}
