using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Persistence;

namespace TaskFlow.Endpoints;

public static class Api
{
    /// <summary>API JSON en lecture, réservée aux utilisateurs connectés (cookie de session).</summary>
    public static void MapTaskFlowApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        api.MapGet("/projects", async (ClaimsPrincipal user, AppDbContext db) =>
            await db.Projects.VisibleTo(user.UserId())
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    Members = p.Members.Count,
                    Tasks = p.Tasks.Count,
                    Done = p.Tasks.Count(t => t.State == Models.TaskState.Done),
                })
                .ToListAsync());

        api.MapGet("/projects/{id:int}/tasks", async (int id, ClaimsPrincipal user, AppDbContext db) =>
        {
            if (await db.FindMembershipAsync(id, user.UserId()) is null)
            {
                return Results.NotFound();
            }

            var tasks = await db.Tasks.Where(t => t.ProjectId == id)
                .OrderBy(t => t.State).ThenBy(t => t.Position)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.State,
                    t.Priority,
                    t.DueDate,
                    Tags = t.Tags,
                    Assignee = t.Assignee == null ? null : t.Assignee.DisplayName,
                    t.CreatedAt,
                    t.CompletedAt,
                })
                .ToListAsync();
            return Results.Ok(tasks);
        });
    }

    public static void MapHealth(this IEndpointRouteBuilder app)
    {
        // Public : permet de vérifier que le conteneur répond et que PostgreSQL est joignable.
        app.MapGet("/health", async (AppDbContext db) =>
        {
            var databaseOk = await db.Database.CanConnectAsync();
            var body = new
            {
                status = databaseOk ? "healthy" : "unhealthy",
                database = databaseOk ? "postgresql ok" : "unreachable",
                version = AppInfo.Version,
                host = Environment.MachineName,
                timeUtc = DateTime.UtcNow,
            };
            return databaseOk ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
        });
    }
}
