using HRDashboard.Models;
using HRDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly GoogleSheetsClient _sheets;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public HomeController(GoogleSheetsClient sheets, IConfiguration config, ApplicationDbContext db)
    {
        _sheets = sheets;
        _config = config;
        _db = db;
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        // Build dashboard stats from database
        var vm = new HomeStatsViewModel();

        var members = _db.SheetMembers?.AsQueryable() ?? Enumerable.Empty<HRDashboard.Models.SheetMember>().AsQueryable();

        // Members by location (Adresse)
        vm.MembersByLocation = members
            .GroupBy(m => string.IsNullOrWhiteSpace(m.Adresse) ? "Unknown" : m.Adresse!)
            .Select(g => new HomeStatsViewModel.StatItem { Key = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();

        var totalMembers = members.Count();
        // Cotisation percentages by status
        vm.CotisationPercentages = members
            .GroupBy(m => string.IsNullOrWhiteSpace(m.CotisationPaymentStatus) ? "Unknown" : m.CotisationPaymentStatus!)
            .Select(g => new HomeStatsViewModel.StatPercentage { Key = g.Key, Count = g.Count(), Percentage = totalMembers > 0 ? Math.Round((double)g.Count() / totalMembers * 100, 1) : 0 })
            .OrderByDescending(p => p.Count)
            .ToList();

        // Department counts
        vm.DepartmentCounts = members
            .GroupBy(m => string.IsNullOrWhiteSpace(m.Department) ? "Unknown" : m.Department!)
            .Select(g => new HomeStatsViewModel.StatItem { Key = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();

        // Overdue tasks (deadline passed and status not done)
        var now = DateTime.UtcNow;
        var tasks = _db.Tasks?.AsQueryable() ?? Enumerable.Empty<HRDashboard.Models.Tasks>().AsQueryable();
        var overdue = 0;
        foreach (var t in tasks)
        {
            if (string.IsNullOrWhiteSpace(t.Deadline)) continue;
            if (!DateTime.TryParse(t.Deadline, out var dt)) continue;
            if (dt.ToUniversalTime() < now)
            {
                var status = (t.Status ?? string.Empty).Trim().ToLowerInvariant();
                if (status != "done" && status != "completed" && status != "closed") overdue++;
            }
        }
        vm.OverdueTasksCount = overdue;

        // Upcoming trainings (workshops)
        vm.UpcomingWorkshops = _db.Workshops?
            .Where(w => w.ScheduledAt > DateTime.UtcNow)
            .OrderBy(w => w.ScheduledAt)
            .Take(5)
            .Select(w => new HomeStatsViewModel.UpcomingWorkshopItem { Id = w.Id, Subject = w.Subject, ScheduledAt = w.ScheduledAt, MailSubject = w.MailSubject })
            .ToList() ?? new List<HomeStatsViewModel.UpcomingWorkshopItem>();

        return View(vm);
    }

}
