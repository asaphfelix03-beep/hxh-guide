using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskFlow.Persistence;
using TaskFlow.Models;

namespace TaskFlow.Pages.Tasks;

public class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public TaskInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var task = new TaskItem();
        Input.ApplyTo(task);
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        TempData["Message"] = "Tâche créée.";
        return RedirectToPage("/Index");
    }
}
