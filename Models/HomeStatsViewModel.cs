using System;
using System.Collections.Generic;

namespace HRDashboard.Models
{
    public class HomeStatsViewModel
    {
        public string Title { get; set; } = "Dashboard";

        public List<StatItem> MembersByLocation { get; set; } = new List<StatItem>();

        public List<StatPercentage> CotisationPercentages { get; set; } = new List<StatPercentage>();

        public List<StatItem> DepartmentCounts { get; set; } = new List<StatItem>();

        public int OverdueTasksCount { get; set; }

        public List<UpcomingWorkshopItem> UpcomingWorkshops { get; set; } = new List<UpcomingWorkshopItem>();

        public class StatItem
        {
            public string Key { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public class StatPercentage
        {
            public string Key { get; set; } = string.Empty;
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public class UpcomingWorkshopItem
        {
            public int Id { get; set; }
            public string Subject { get; set; } = string.Empty;
            public DateTime ScheduledAt { get; set; }
            public string? MailSubject { get; set; }
        }
    }
}
