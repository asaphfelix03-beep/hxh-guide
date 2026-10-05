using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Persistence;
using TaskFlow.Models;

namespace TaskFlow.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    /// <summary>Filtre courant, lu dans l'URL : ?status=active ou ?status=done.</summary>
    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public List<TaskItem> Tasks { get; private set; } = [];
    public int AllCount { get; private set; }
    public int ActiveCount { get; private set; }
    public int DoneCount { get; private set; }

    public async Task OnGetAsync()
    {
        Tasks = await db.Tasks.WithStatus(Status).InDisplayOrder().ToListAsync();
        AllCount = await db.Tasks.CountAsync();
        DoneCount = await db.Tasks.CountAsync(t => t.IsDone);
        ActiveCount = AllCount - DoneCount;
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        task.IsDone = !task.IsDone;
        await db.SaveChangesAsync();
        return RedirectToPage(new { status = Status });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await db.Tasks.Where(t => t.Id == id).ExecuteDeleteAsync();
        if (deleted == 0)
        {
            return NotFound();
        }

        TempData["Message"] = "Tâche supprimée.";
        return RedirectToPage(new { status = Status });
    }
}
