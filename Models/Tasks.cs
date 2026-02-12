using System;
using System.ComponentModel.DataAnnotations;

namespace HRDashboard.Models
{
    public class Tasks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public string? Department{ get; set; }
        [Required]
        public string? MemberId { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string? Deadline { get; set; }
        
        public string? Status { get; set; }
        
    }
}
