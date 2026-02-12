using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRDashboard.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, dashes and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        // Profile Picture Upload Field - Made optional
        
        [DataType(DataType.Upload)]
        [Required]
        public IFormFile? ProfileImage { get; set; }

        // This will store the Cloudinary URL returned after upload - Not required for input
        public string? ProfilePicture { get; set; }

        // Role selection property
        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;
    }
}