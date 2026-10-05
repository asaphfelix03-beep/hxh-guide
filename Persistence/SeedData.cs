using TaskFlow.Models;

namespace TaskFlow.Persistence;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        // EnsureCreated renvoie false si la base existe déjà : on ne réinsère rien.
        if (!db.Database.EnsureCreated())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "Écrire le Dockerfile",
                Description = "Build multi-étapes : image SDK pour compiler, image ASP.NET pour exécuter.",
                Priority = Priority.High,
                IsDone = true,
            },
            new TaskItem
            {
                Title = "Déployer sur une instance EC2",
                Description = "Installer Docker, lancer le conteneur et ouvrir le port 80 dans le Security Group.",
                Priority = Priority.High,
                DueDate = today.AddDays(2),
            },
            new TaskItem
            {
                Title = "Préparer le rendu",
                Description = "Captures : console EC2, docker ps, application dans le navigateur.",
                DueDate = today.AddDays(5),
            });
        db.SaveChanges();
    }
}
