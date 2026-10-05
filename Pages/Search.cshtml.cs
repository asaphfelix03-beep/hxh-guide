using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages;

public class SearchModel(AppDbContext db) : PageModel
{
    private const int MaxResults = 50;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public TaskState? State { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Mine { get; set; }

    public bool Searched { get; private set; }
    public List<TaskItem> Results { get; private set; } = [];
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task OnGetAsync()
    {
        var text = Q?.Trim().TrimStart('#');
        Searched = !string.IsNullOrEmpty(text) || State is not null || Mine;
        if (!Searched)
        {
            return;
        }

        var userId = User.UserId();
        var query = db.Tasks.VisibleTo(userId);

        if (!string.IsNullOrEmpty(text))
        {
            // ILIKE PostgreSQL : insensible à la casse. On neutralise les jokers % et _ saisis par l'utilisateur.
            var pattern = "%" + text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            query = query.Where(t =>
                EF.Functions.ILike(t.Title, pattern)
                || EF.Functions.ILike(t.Description ?? "", pattern)
                || EF.Functions.ILike(t.Tags, pattern));
        }
        if (State is { } state)
        {
            query = query.Where(t => t.State == state);
        }
        if (Mine)
        {
            query = query.Where(t => t.AssigneeId == userId);
        }

        Results = await query
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .OrderBy(t => t.State == TaskState.Done)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .Take(MaxResults)
            .AsNoTracking()
            .ToListAsync();
    }
}
