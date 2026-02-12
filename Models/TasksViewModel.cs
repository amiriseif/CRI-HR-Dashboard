using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRDashboard.Models
{
    public class TasksViewModel
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string? Department { get; set; }
    public string? MemberId { get; set; }
    public string? Description { get; set; }
    public string? Deadline { get; set; }     // keeping as string like your model
    public string? Status { get; set; }       // "ToDo", "InProgress", "Done", "Blocked"
}

// CreateTaskViewModel.cs   ← used for form binding
public class CreateTaskViewModel
{
    [Required]
    public string? Department { get; set; }

    [Required]
    [Display(Name = "Assigned Member")]
    public string? MemberId { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public string? Deadline { get; set; }     // or DateOnly / DateTime

    public string Status { get; set; } = "ToDo";
}

// Simple DTO for status update via AJAX
public class UpdateTaskStatusDto
{
    public string Status { get; set; } = string.Empty;
}
}