using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using HRDashboard.Models;  
using System.Threading.Tasks;
public interface IUserRegistrationService
{
    Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
}

public class UserRegistrationService : IUserRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserRegistrationService> _logger;

    public UserRegistrationService(UserManager<ApplicationUser> userManager, 
                                   SignInManager<ApplicationUser> signInManager, 
                                   RoleManager<IdentityRole> roleManager,
                                   ILogger<UserRegistrationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Username,
            FullName = model.FullName,
            ProfilePicture = model.ProfilePicture
        };
        _logger.LogInformation("starting");

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("Creating user with username: {Username}", model.Username);
            var role = model.SelectedRole;
            await _userManager.AddToRoleAsync(user, role);
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        return result;
    }
}
