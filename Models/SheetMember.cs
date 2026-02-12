using System;
using System.ComponentModel.DataAnnotations;

namespace HRDashboard.Models
{
    public class SheetMember
    {
        [Key]
        public int Id { get; set; }

        // External id from the Google Sheet (first column)
        [Required]
        public string ExternalId { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Adresse { get; set; }
        public string? TypeMembership { get; set; }
        public string? MembershipStatus { get; set; }
        public string? Department { get; set; }
        public string? CotisationPaymentStatus { get; set; }

        public DateTime LastSyncedAt { get; set; }
    }
}
