using System;
using System.ComponentModel.DataAnnotations;

namespace HRDashboard.Models
{
    public class Workshop
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Trainer { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledAt { get; set; }

        // "presentiel" or "online"
        [Required]
        public string Type { get; set; } = string.Empty;

        // optional: location for presentiel or meet link for online
        public string? LocationOrLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Generated mail subject and body from automation
        public string? MailSubject { get; set; }
        public string? MailBody { get; set; }
    }
}
