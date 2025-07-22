using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> rolemanager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = rolemanager;
        }

       
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList()
            };
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList();

                return View(model);
            }
            model.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Book");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);

        }
        

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Book");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}


