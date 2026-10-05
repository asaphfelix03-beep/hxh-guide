using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Persistence;

/// <summary>
/// Règle de sécurité centrale : un utilisateur ne voit que les projets dont il est membre,
/// et seulement les tâches de ces projets.
/// </summary>
public static class AccessQueries
{
    public static IQueryable<Project> VisibleTo(this IQueryable<Project> query, string userId) =>
        query.Where(p => p.Members.Any(m => m.UserId == userId));

    public static IQueryable<TaskItem> VisibleTo(this IQueryable<TaskItem> query, string userId) =>
        query.Where(t => t.Project.Members.Any(m => m.UserId == userId));

    public static Task<ProjectMember?> FindMembershipAsync(this AppDbContext db, int projectId, string userId) =>
        db.ProjectMembers.Include(m => m.Project).FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

    /// <summary>Tâche accessible à l'utilisateur, avec son projet, sinon null.</summary>
    public static Task<TaskItem?> FindVisibleTaskAsync(this AppDbContext db, int taskId, string userId) =>
        db.Tasks.VisibleTo(userId).Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId);

    public static string UserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Utilisateur non connecté.");
}
