using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HRDashboard.Models;


public class LogoutController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    // GET: /Logout
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home"); // Redirect to Home after logout
    }
}
