using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HRDashboard.Models;

namespace HRDashboard.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Models.SheetMember>? SheetMembers { get; set; }
        public DbSet<Models.Tasks>? Tasks { get; set; }
        public DbSet<Models.Workshop>? Workshops { get; set; }
    }
}