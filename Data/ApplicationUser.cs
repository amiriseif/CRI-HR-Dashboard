using Microsoft.AspNetCore.Identity;

namespace HRDashboard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public String Fullname { get; set; }
        public String ProfilePicture { get; set; }
    }
}