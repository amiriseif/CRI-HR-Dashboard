using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HRDashboard.Models;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
public class LoginController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager; // or your User class name

    public LoginController(SignInManager<ApplicationUser> signInManager,UserManager<ApplicationUser> userManager)
    {
      _signInManager= signInManager;
      _userManager  = userManager;
    }
      [HttpGet]
    public IActionResult Index() => View();

    // POST: /Login
    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
            if (ModelState.IsValid)
            {
                // Your authentication logic here
                var user = await _userManager.FindByNameAsync(model.UserName);
                
                if (user != null)
                {
                    var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (passwordCheck.Succeeded)
                    {
                        // Get user role
                        var roles = await _userManager.GetRolesAsync(user);
                        var userRole = roles.FirstOrDefault() ?? "User";

                        // Add profile picture (and optionally full name) as claims so they persist in the auth cookie
                        var additionalClaims = new List<Claim>();
                        if (!string.IsNullOrEmpty(user.ProfilePicture))
                        {
                            additionalClaims.Add(new Claim("ProfilePicture", user.ProfilePicture));
                        }
                        if (!string.IsNullOrEmpty(user.FullName))
                        {
                            additionalClaims.Add(new Claim("FullName", user.FullName));
                        }

                        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, additionalClaims);

                        // Keep TempData for the one-time welcome popup if you want
                        TempData["WelcomeUser"] = user.FullName;
                        TempData["UserRole"] = userRole;
                        TempData["ShowWelcome"] = true;

                        return RedirectToAction("Index", "Home");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

        return View(model);
    }
}