using Application.Common.Interfaces;
using Application.Common.Utility;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
             RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            LoginVM loginVM = new LoginVM
            {
                RedirectUrl = returnUrl
            };

            return View(loginVM);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM obj)
        {
            if (ModelState.IsValid)
            {
                var result = _signInManager.PasswordSignInAsync(obj.Email, obj.Password, obj.RememberMe, lockoutOnFailure: false).Result;
                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(obj.RedirectUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return LocalRedirect(obj.RedirectUrl);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Login Attempt");
                }
            }



            return View(obj);
        }
        public IActionResult Register(string ReturnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).Wait();
            }

            RegisterVM registerVM = new RegisterVM()
            {
                RolesList = _roleManager.Roles.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                RedirectUrl = ReturnUrl
            };


            return View(registerVM);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {

                ApplicationUser usr = new()
                {
                    Name = registerVM.Name,
                    Email = registerVM.Email,
                    PhoneNumber = registerVM.PhoneNumber,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerVM.Email,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(usr, registerVM.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(registerVM.Role))
                    {
                        await _userManager.AddToRoleAsync(usr, registerVM.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(usr, SD.Role_Customer);
                    }
                    await _signInManager.SignInAsync(usr, isPersistent: false);
                    if (string.IsNullOrEmpty(registerVM.Role))
                    {
                        return RedirectToAction("Index", "Home");

                    }
                    else
                    {
                        return LocalRedirect(registerVM.RedirectUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }



            registerVM.RolesList = _roleManager.Roles.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });


            return View(registerVM);
        }

        public IActionResult Denied()
        {
            return View();
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }
    }
}
