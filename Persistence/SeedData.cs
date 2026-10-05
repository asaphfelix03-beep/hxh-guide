using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Persistence;

/// <summary>
/// Comptes et projet de démonstration (activés par Seed__Demo=true), pour tester l'application sans s'inscrire.
/// </summary>
public static class SeedData
{
    public const string DemoEmail = "demo@taskflow.local";
    public const string DemoPassword = "Demo1234";

    public static async Task SeedDemoAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var users = services.GetRequiredService<UserManager<AppUser>>();
        var demo = await CreateUserAsync(users, DemoEmail, "Alex Demo");
        var camille = await CreateUserAsync(users, "camille@taskflow.local", "Camille Martin");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var project = new Project
        {
            Name = "TP Cloud",
            Description = "Conteneuriser une application et la déployer sur AWS.",
            Members =
            [
                new ProjectMember { UserId = demo.Id, Role = ProjectRole.Owner },
                new ProjectMember { UserId = camille.Id, Role = ProjectRole.Member },
            ],
        };
        db.Projects.Add(project);

        var position = 0;
        TaskItem NewTask(string title, TaskState state, Priority priority, string tags, AppUser? assignee, int? dueInDays = null, string? description = null)
        {
            var task = new TaskItem
            {
                Project = project,
                Title = title,
                Description = description,
                Priority = priority,
                Tags = TagList.Normalize(tags),
                AssigneeId = assignee?.Id,
                CreatedById = demo.Id,
                DueDate = dueInDays is { } days ? today.AddDays(days) : null,
                Position = position++,
            };
            task.MoveTo(state);
            return task;
        }

        db.Tasks.AddRange(
            NewTask("Rédiger le rapport", TaskState.Todo, Priority.High, "doc", demo, 3, "Architecture, choix techniques, captures d'écran."),
            NewTask("Préparer la démo", TaskState.Todo, Priority.Normal, "doc, oral", camille, 5),
            NewTask("Configurer les alertes de budget AWS", TaskState.Todo, Priority.Low, "aws", null),
            NewTask("Mettre en place HTTPS avec Caddy", TaskState.InProgress, Priority.High, "infra, sécurité", demo, 1),
            NewTask("Passer à PostgreSQL", TaskState.InProgress, Priority.Normal, "infra, base de données", camille, -1),
            NewTask("Écrire le Dockerfile", TaskState.Done, Priority.High, "docker", demo),
            NewTask("Créer l'instance EC2", TaskState.Done, Priority.High, "aws", camille),
            NewTask("Publier l'image sur Docker Hub", TaskState.Done, Priority.Normal, "docker", demo));

        await db.SaveChangesAsync();
    }

    private static async Task<AppUser> CreateUserAsync(UserManager<AppUser> users, string email, string name)
    {
        var user = new AppUser { UserName = email, Email = email, DisplayName = name, EmailConfirmed = true };
        var result = await users.CreateAsync(user, DemoPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
        return user;
    }
}
