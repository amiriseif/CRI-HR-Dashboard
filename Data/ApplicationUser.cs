using Microsoft.AspNetCore.Identity;

namespace HRDashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required String FullName { get; set; }
        
        public required String ProfilePicture { get; set; }
    }
}