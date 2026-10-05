using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Tasks;

/// <summary>Code commun aux pages de création et de modification de tâche.</summary>
public abstract class TaskFormPage(AppDbContext db) : PageModel
{
    protected AppDbContext Db { get; } = db;

    [BindProperty]
    public TaskInput Input { get; set; } = new();

    public Project Project { get; protected set; } = null!;

    /// <summary>Choix « Assignée à » : uniquement les membres du projet.</summary>
    public List<SelectListItem> MemberOptions { get; private set; } = [];

    protected async Task LoadMembersAsync(int projectId)
    {
        MemberOptions = await Db.ProjectMembers.Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.User.DisplayName)
            .Select(m => new SelectListItem(m.User.DisplayName, m.UserId))
            .ToListAsync();
    }

    /// <summary>Refuse une assignation à quelqu'un qui n'est pas membre du projet.</summary>
    protected void ValidateAssignee()
    {
        if (!string.IsNullOrEmpty(Input.AssigneeId) && MemberOptions.All(m => m.Value != Input.AssigneeId))
        {
            ModelState.AddModelError("Input.AssigneeId", "Cette personne ne fait pas partie du projet.");
        }
    }

    /// <summary>Position à la fin de la colonne.</summary>
    protected async Task<int> NextPositionAsync(int projectId, TaskState state) =>
        (await Db.Tasks.Where(t => t.ProjectId == projectId && t.State == state).MaxAsync(t => (int?)t.Position) ?? -1) + 1;
}
