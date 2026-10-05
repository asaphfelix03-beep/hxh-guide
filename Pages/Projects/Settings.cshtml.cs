using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Persistence;

namespace TaskFlow.Pages.Projects;

public class SettingsModel(AppDbContext db, UserManager<AppUser> users) : PageModel
{
    public Project Project { get; private set; } = null!;
    public bool IsOwner { get; private set; }
    public string CurrentUserId { get; private set; } = "";
    public List<MemberRow> Members { get; private set; } = [];

    [BindProperty]
    public ProjectInput Input { get; set; } = new();

    public record MemberRow(string UserId, string DisplayName, string? Email, ProjectRole Role, int OpenTasks);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        Input = new ProjectInput { Name = Project.Name, Description = Project.Description };
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        if (!IsOwner)
        {
            return Forbid();
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Input.ApplyTo(Project);
        await db.SaveChangesAsync();
        TempData["Message"] = "Projet mis à jour.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddMemberAsync(int id, string? email)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        if (!IsOwner)
        {
            return Forbid();
        }

        var user = string.IsNullOrWhiteSpace(email) ? null : await users.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            TempData["Error"] = "Aucun compte avec cette adresse. La personne doit d'abord créer son compte TaskFlow.";
        }
        else if (Members.Any(m => m.UserId == user.Id))
        {
            TempData["Error"] = $"{user.DisplayName} fait déjà partie du projet.";
        }
        else
        {
            db.ProjectMembers.Add(new ProjectMember { ProjectId = id, UserId = user.Id, Role = ProjectRole.Member });
            await db.SaveChangesAsync();
            TempData["Message"] = $"{user.DisplayName} a rejoint le projet.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveMemberAsync(int id, string userId)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        if (!IsOwner)
        {
            return Forbid();
        }

        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null || member.Role == ProjectRole.Owner)
        {
            TempData["Error"] = "Le propriétaire ne peut pas être retiré du projet.";
            return RedirectToPage(new { id });
        }

        await RemoveMemberAsync(id, userId);
        TempData["Message"] = $"{member.DisplayName} a été retiré du projet. Ses tâches ne sont plus assignées.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostLeaveAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        if (IsOwner)
        {
            TempData["Error"] = "Le propriétaire ne peut pas quitter son projet : supprimez-le à la place.";
            return RedirectToPage(new { id });
        }

        await RemoveMemberAsync(id, CurrentUserId);
        TempData["Message"] = $"Vous avez quitté le projet « {Project.Name} ».";
        return RedirectToPage("/Projects/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? confirmName)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }
        if (!IsOwner)
        {
            return Forbid();
        }
        if (!string.Equals(confirmName?.Trim(), Project.Name, StringComparison.Ordinal))
        {
            TempData["Error"] = "Le nom saisi ne correspond pas : le projet n'a pas été supprimé.";
            return RedirectToPage(new { id });
        }

        db.Projects.Remove(Project); // supprime aussi ses tâches et ses membres (cascade)
        await db.SaveChangesAsync();
        TempData["Message"] = $"Projet « {Project.Name} » supprimé.";
        return RedirectToPage("/Projects/Index");
    }

    /// <summary>Charge le projet si l'utilisateur en est membre.</summary>
    private async Task<bool> LoadAsync(int id)
    {
        CurrentUserId = User.UserId();
        var membership = await db.FindMembershipAsync(id, CurrentUserId);
        if (membership is null)
        {
            return false;
        }

        Project = membership.Project;
        IsOwner = membership.Role == ProjectRole.Owner;
        Members = await db.ProjectMembers.Where(m => m.ProjectId == id)
            .OrderByDescending(m => m.Role).ThenBy(m => m.User.DisplayName)
            .Select(m => new MemberRow(
                m.UserId,
                m.User.DisplayName,
                m.User.Email,
                m.Role,
                db.Tasks.Count(t => t.ProjectId == id && t.AssigneeId == m.UserId && t.State != TaskState.Done)))
            .ToListAsync();
        return true;
    }

    private async Task RemoveMemberAsync(int projectId, string userId)
    {
        await db.Tasks.Where(t => t.ProjectId == projectId && t.AssigneeId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.AssigneeId, (string?)null));
        await db.ProjectMembers.Where(m => m.ProjectId == projectId && m.UserId == userId).ExecuteDeleteAsync();
    }
}
