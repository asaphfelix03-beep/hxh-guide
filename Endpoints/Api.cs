using System.Security.Claims;
using HxhGuide.Models;
using HxhGuide.Persistence;

namespace HxhGuide.Endpoints;

public static class Api
{
    public static void MapGuideApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        // Public : les arcs avec la note de la communauté.
        api.MapGet("/arcs", async (AppDbContext db) =>
        {
            var ratings = await db.ArcRatingsAsync();
            return Catalog.Arcs.Select(arc => new
            {
                arc.Slug,
                arc.Title,
                arc.FirstVolume,
                arc.LastVolume,
                arc.Ongoing,
                Rating = ratings.TryGetValue(arc.Slug, out var r) ? Math.Round(r.Average, 1) : (double?)null,
                Reviews = ratings.TryGetValue(arc.Slug, out var c) ? c.Count : 0,
            });
        });

        // Privé : la progression du lecteur connecté.
        api.MapGet("/moi", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            var state = await db.ReaderStateAsync(user);
            return new
            {
                Name = user.DisplayName(),
                Read = state.Read.Order(),
                state.ReadCount,
                state.Percent,
                state.NextVolume,
                CurrentArc = state.CurrentArc?.Slug,
                state.HideSpoilers,
            };
        }).RequireAuthorization();
    }

    public static void MapHealth(this IEndpointRouteBuilder app)
    {
        // Public : permet de vérifier que le service répond et que la base est joignable.
        app.MapGet("/health", async (AppDbContext db) =>
        {
            var databaseOk = await db.Database.CanConnectAsync();
            var body = new
            {
                status = databaseOk ? "healthy" : "unhealthy",
                database = databaseOk ? "ok" : "unreachable",
                version = AppInfo.Version,
                timeUtc = DateTime.UtcNow,
            };
            return databaseOk ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
        });
    }
}
