using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRDashboard.Data;                    // your DbContext
using HRDashboard.Models;                  // your Task entity
using HRDashboard.Models;

namespace YourApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Tasks/
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks
                .OrderByDescending(t => t.CreationDate)
                .ToListAsync();

            var viewModels = tasks.Select(t => new TasksViewModel
            {
                Id = t.Id,
                CreationDate = t.CreationDate,
                Department = t.Department,
                MemberId = t.MemberId,
                Description = t.Description,
                Deadline = t.Deadline,
                Status = t.Status
            });

            return View(viewModels);
        }

        // POST: /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If invalid → re-show the list + form errors
                var tasks = await _context.Tasks.OrderByDescending(t => t.CreationDate).ToListAsync();
                return View("Index", tasks.Select(t => new TasksViewModel { /* mapping */ }));
            }

            var task = new Tasks
            {
                CreationDate = DateTime.UtcNow,           // or DateTime.Now
                Department = model.Department,
                MemberId = model.MemberId,
                Description = model.Description,
                Deadline = model.Deadline,
                Status = model.Status
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Tasks/UpdateStatus/5
        [HttpPost]
        [Route("Tasks/UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Status))
                return BadRequest("Status is required");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            // Optional: validate allowed statuses
            var allowed = new[] { "ToDo", "InProgress", "Done", "Blocked" };
            if (!allowed.Contains(dto.Status))
                return BadRequest("Invalid status");

            task.Status = dto.Status;
            // task.UpdatedAt = DateTime.UtcNow;  ← if you have this field

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}