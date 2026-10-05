using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages;

public class IndexModel(AppDbContext db, IConfiguration configuration) : PageModel
{
    public bool DemoEnabled => configuration.GetValue<bool>("Seed:Demo");

    public string DisplayName { get; private set; } = "";
    public int OpenCount { get; private set; }
    public int OverdueCount { get; private set; }
    public int DueSoonCount { get; private set; }
    public int DoneThisWeekCount { get; private set; }
    public List<TaskItem> MyTasks { get; private set; } = [];
    public List<ProjectSummary> Projects { get; private set; } = [];
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return; // page d'accueil publique
        }

        var userId = User.UserId();
        DisplayName = User.FindFirstValue(AppClaimsPrincipalFactory.DisplayNameClaim) ?? "";

        var inAWeek = Today.AddDays(7);
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var myOpen = db.Tasks.VisibleTo(userId).Where(t => t.AssigneeId == userId && t.State != TaskState.Done);

        OpenCount = await myOpen.CountAsync();
        OverdueCount = await myOpen.CountAsync(t => t.DueDate < Today);
        DueSoonCount = await myOpen.CountAsync(t => t.DueDate >= Today && t.DueDate <= inAWeek);
        DoneThisWeekCount = await db.Tasks.VisibleTo(userId).CountAsync(t => t.CompletedAt >= weekAgo);

        MyTasks = await myOpen
            .Include(t => t.Project)
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Take(8)
            .AsNoTracking()
            .ToListAsync();

        Projects = await db.Projects.VisibleTo(userId)
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
