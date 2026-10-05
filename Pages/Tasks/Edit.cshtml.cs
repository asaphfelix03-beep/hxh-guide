using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Persistence;
using TaskFlow.Models;

namespace TaskFlow.Pages.Tasks;

public class EditModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public TaskInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        Input = TaskInput.From(task);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Input.ApplyTo(task);
        await db.SaveChangesAsync();

        TempData["Message"] = "Tâche mise à jour.";
        return RedirectToPage("/Index");
    }
}
