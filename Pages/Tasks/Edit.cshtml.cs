using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Tasks;

public class EditModel(AppDbContext db) : TaskFormPage(db)
{
    public TaskItem Item { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        Input = TaskInput.From(Item);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        ValidateAssignee();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.State != Item.State)
        {
            Item.Position = await NextPositionAsync(Item.ProjectId, Input.State); // arrive en bas de sa nouvelle colonne
        }
        Input.ApplyTo(Item);
        await Db.SaveChangesAsync();

        TempData["Message"] = "Tâche mise à jour.";
        return RedirectToPage("/Projects/Board", new { id = Item.ProjectId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        Db.Tasks.Remove(Item);
        await Db.SaveChangesAsync();

        TempData["Message"] = $"Tâche « {Item.Title} » supprimée.";
        return RedirectToPage("/Projects/Board", new { id = Item.ProjectId });
    }

    private async Task<bool> LoadAsync(int id)
    {
        var task = await Db.Tasks.VisibleTo(User.UserId())
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
        {
            return false;
        }
        Item = task;
        Project = task.Project;
        await LoadMembersAsync(task.ProjectId);
        return true;
    }
}
