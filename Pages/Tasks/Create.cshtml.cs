using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Tasks;

public class CreateModel(AppDbContext db) : TaskFormPage(db)
{
    public async Task<IActionResult> OnGetAsync(int projectId, TaskState state = TaskState.Todo)
    {
        if (!await LoadAsync(projectId))
        {
            return NotFound();
        }
        Input.State = state;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectId)
    {
        if (!await LoadAsync(projectId))
        {
            return NotFound();
        }
        ValidateAssignee();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var task = new TaskItem
        {
            ProjectId = projectId,
            CreatedById = User.UserId(),
            Position = await NextPositionAsync(projectId, Input.State),
        };
        Input.ApplyTo(task);
        Db.Tasks.Add(task);
        await Db.SaveChangesAsync();

        TempData["Message"] = $"Tâche « {task.Title} » créée.";
        return RedirectToPage("/Projects/Board", new { id = projectId });
    }

    private async Task<bool> LoadAsync(int projectId)
    {
        var membership = await Db.FindMembershipAsync(projectId, User.UserId());
        if (membership is null)
        {
            return false;
        }
        Project = membership.Project;
        await LoadMembersAsync(projectId);
        return true;
    }
}
