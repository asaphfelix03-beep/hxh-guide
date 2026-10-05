using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Persistence;
using TaskFlow.Models;

namespace TaskFlow.Endpoints;

public static class TaskApi
{
    public static void MapTaskApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/tasks");

        // GET /api/tasks?status=active|done
        api.MapGet("/", async (string? status, AppDbContext db) =>
            await db.Tasks.WithStatus(status).InDisplayOrder().ToListAsync());

        api.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Tasks.FindAsync(id) is { } task ? Results.Ok(task) : Results.NotFound());

        api.MapPost("/", async (TaskInput input, AppDbContext db) =>
        {
            if (Validate(input) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var task = new TaskItem();
            input.ApplyTo(task);
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tasks/{task.Id}", task);
        });

        api.MapPut("/{id:int}", async (int id, TaskInput input, AppDbContext db) =>
        {
            if (Validate(input) is { } errors)
            {
                return Results.ValidationProblem(errors);
            }

            var task = await db.Tasks.FindAsync(id);
            if (task is null)
            {
                return Results.NotFound();
            }

            input.ApplyTo(task);
            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        api.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var deleted = await db.Tasks.Where(t => t.Id == id).ExecuteDeleteAsync();
            return deleted == 0 ? Results.NotFound() : Results.NoContent();
        });
    }

    public static void MapHealth(this IEndpointRouteBuilder app)
    {
        // Permet de vérifier que le conteneur répond et que la base est accessible.
        app.MapGet("/health", async (AppDbContext db) =>
        {
            var databaseOk = await db.Database.CanConnectAsync();
            var body = new
            {
                status = databaseOk ? "healthy" : "unhealthy",
                database = databaseOk ? "ok" : "unreachable",
                version = AppInfo.Version,
                host = Environment.MachineName,
                timeUtc = DateTime.UtcNow,
            };
            return databaseOk ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
        });
    }

    private static Dictionary<string, string[]>? Validate(TaskInput input)
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true))
        {
            return null;
        }

        return results
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Valeur invalide.").ToArray());
    }
}
