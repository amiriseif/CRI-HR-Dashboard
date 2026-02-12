// File: Models/RegisterViewModel.cs

namespace HRDashboard.Models
{
    public class LoginViewModel
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
        
    }
}
