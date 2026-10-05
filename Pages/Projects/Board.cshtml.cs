using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Projects;

public class BoardModel(AppDbContext db) : PageModel
{
    public Project Project { get; private set; } = null!;
    public string CurrentUserId { get; private set; } = "";
    public List<MemberChip> Members { get; private set; } = [];
    public Dictionary<TaskState, List<TaskItem>> Columns { get; private set; } = [];
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        CurrentUserId = User.UserId();
        var membership = await db.FindMembershipAsync(id, CurrentUserId);
        if (membership is null)
        {
            return NotFound();
        }

        Project = membership.Project;
        Members = await db.ProjectMembers.Where(m => m.ProjectId == id)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new MemberChip(m.UserId, m.User.DisplayName))
            .ToListAsync();

        var tasks = await db.Tasks.Where(t => t.ProjectId == id)
            .Include(t => t.Assignee)
            .OrderBy(t => t.Position).ThenBy(t => t.Id)
            .AsNoTracking()
            .ToListAsync();
        Columns = Enum.GetValues<TaskState>().ToDictionary(state => state, state => tasks.Where(t => t.State == state).ToList());
        return Page();
    }

    /// <summary>Appelé par wwwroot/js/board.js après un glisser-déposer.</summary>
    public async Task<IActionResult> OnPostMoveAsync(int id, int taskId, TaskState state, int index)
    {
        if (await db.FindMembershipAsync(id, User.UserId()) is null)
        {
            return NotFound();
        }
        if (!Enum.IsDefined(state))
        {
            return BadRequest();
        }

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == id);
        if (task is null)
        {
            return NotFound();
        }

        // Réinsère la tâche à sa nouvelle place, puis renumérote la colonne cible.
        var column = await db.Tasks
            .Where(t => t.ProjectId == id && t.State == state && t.Id != taskId)
            .OrderBy(t => t.Position).ThenBy(t => t.Id)
            .ToListAsync();
        column.Insert(Math.Clamp(index, 0, column.Count), task);
        task.MoveTo(state);
        for (var i = 0; i < column.Count; i++)
        {
            column[i].Position = i;
        }

        await db.SaveChangesAsync();
        return new JsonResult(new { ok = true, state = state.ToString(), done = task.State == TaskState.Done });
    }

    public async Task<IActionResult> OnPostQuickAddAsync(int id, string? title, TaskState state)
    {
        var userId = User.UserId();
        if (await db.FindMembershipAsync(id, userId) is null)
        {
            return NotFound();
        }

        title = title?.Trim();
        if (string.IsNullOrEmpty(title) || title.Length > 100 || !Enum.IsDefined(state))
        {
            TempData["Error"] = "Le titre est obligatoire (100 caractères maximum).";
            return RedirectToPage(new { id });
        }

        var lastPosition = await db.Tasks.Where(t => t.ProjectId == id && t.State == state).MaxAsync(t => (int?)t.Position) ?? -1;
        var task = new TaskItem { ProjectId = id, Title = title, Position = lastPosition + 1, CreatedById = userId };
        task.MoveTo(state);
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }
}
