using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HRDashboard.Models;
using HRDashboard.Services;

public class RegisterController : Controller
{
    private readonly IUserRegistrationService _userRegistrationService;
    private readonly ILogger<RegisterController> _logger;
    private readonly ICloudinaryService _cloudinaryService;

    public RegisterController(
        IUserRegistrationService userRegistrationService, 
        ILogger<RegisterController> logger, 
        ICloudinaryService cloudinaryService)
    {
        _userRegistrationService = userRegistrationService;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
    }

    // GET: /Register
    [HttpGet]
    public IActionResult Index() => View();

    // POST: /Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Index(RegisterViewModel model)
    {
        _logger.LogInformation("Register button clicked. Username: {Username}", model?.Username ?? "(null)");
        
        if (ModelState.IsValid)
        {
            _logger.LogInformation("model valid for user: {Username}", model.Username);
            
            // Handle profile picture upload if provided
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                try
                {
                    _logger.LogInformation("Uploading profile picture for user: {Username}", model.Username);
                    
                    // Upload to Cloudinary
                    string cloudinaryUrl = await _cloudinaryService.UploadImageAsync(model.ProfileImage);
                    
                    // Store the URL in the ProfilePicture property
                    model.ProfilePicture = cloudinaryUrl;
                    
                    _logger.LogInformation("Profile picture uploaded successfully. URL received.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading profile picture for user: {Username}", model.Username);
                    ModelState.AddModelError("ProfileImage", $"Error uploading image: {ex.Message}");
                    return View(model);
                }
            }
            else
            {
                _logger.LogInformation("No profile picture provided for user: {Username}", model.Username);
                // You could set a default profile picture URL here if you want
                // model.ProfilePicture = "/images/default-avatar.png";
            }
            
            // Pass the model (which now contains ProfilePicture URL) to your service
            var result = await _userRegistrationService.RegisterUserAsync(model);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} created successfully.", model.Username);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    _logger.LogError("Error during user creation: {ErrorDescription}", error.Description);
                }
            }
        }
        else
        {
            _logger.LogInformation("model not valid");
            // Log validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            _logger.LogDebug("Validation errors: {Errors}", 
                string.Join(", ", errors.Select(e => e.ErrorMessage)));
        }
        
        return View(model);
    }
    

    // Optional: Add a method to validate file before upload
    private bool ValidateProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // Check file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024) // 5MB
        {
            ModelState.AddModelError("ProfilePicture", "File size cannot exceed 5MB");
            return false;
        }

        // Check file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            ModelState.AddModelError("ProfilePicture", 
                $"Allowed file types: {string.Join(", ", allowedExtensions)}");
            return false;
        }

        // Check MIME type
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            ModelState.AddModelError("ProfilePicture", "Invalid file type");
            return false;
        }

        return true;
    }
}