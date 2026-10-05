using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Projects;

public class IndexModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public ProjectInput Input { get; set; } = new();

    public List<ProjectSummary> Projects { get; private set; } = [];

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var project = new Project { Members = [new ProjectMember { UserId = User.UserId(), Role = ProjectRole.Owner }] };
        Input.ApplyTo(project);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        TempData["Message"] = $"Projet « {project.Name} » créé. Ajoutez vos premières tâches !";
        return RedirectToPage("/Projects/Board", new { id = project.Id });
    }

    private async Task LoadAsync()
    {
        Projects = await db.Projects.VisibleTo(User.UserId())
            .OrderBy(p => p.Name)
            .Select(p => new ProjectSummary(
                p.Id,
                p.Name,
                p.Description,
                p.Tasks.Count,
                p.Tasks.Count(t => t.State == TaskState.Done),
                p.Members.OrderBy(m => m.JoinedAt).Select(m => new MemberChip(m.UserId, m.User.DisplayName)).ToList()))
            .ToListAsync();
    }
}
