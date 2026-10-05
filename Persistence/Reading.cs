using System.Security.Claims;
using HxhGuide.Models;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Persistence;

/// <summary>Où en est le lecteur : tomes lus et protection contre les spoilers.</summary>
public record ReaderState(bool SignedIn, string? UserId, bool HideSpoilers, IReadOnlySet<int> Read)
{
    public static readonly ReaderState Anonymous = new(false, null, true, new HashSet<int>());

    public int ReadCount => Read.Count;
    public int Percent => (int)Math.Round(100.0 * ReadCount / Catalog.TotalVolumes);
    public int MaxRead => Read.Count == 0 ? 0 : Read.Max();

    /// <summary>Premier tome non lu (null si tout est lu).</summary>
    public int? NextVolume => Enumerable.Range(1, Catalog.TotalVolumes).Cast<int?>().FirstOrDefault(v => !Read.Contains(v!.Value));

    public Arc? CurrentArc => NextVolume is { } next ? Catalog.MainArcOfVolume(next) : null;

    public int ArcRead(Arc arc) => Read.Count(arc.Contains);
    public int ArcPercent(Arc arc) => (int)Math.Round(100.0 * ArcRead(arc) / arc.VolumeCount);

    /// <summary>
    /// Vrai si le contenu de l'arc peut s'afficher directement : le lecteur l'a atteint,
    /// ou il a désactivé la protection. Sinon, le contenu reste disponible derrière un clic.
    /// </summary>
    public bool CanSee(Arc arc) => SignedIn && (!HideSpoilers || MaxRead >= arc.FirstVolume);

    public bool CanSee(Character character) => CanSee(character.FirstArc);
}

public static class Reading
{
    public static string? UserIdOrNull(this ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    public static string UserId(this ClaimsPrincipal user) =>
        user.UserIdOrNull() ?? throw new InvalidOperationException("Utilisateur non connecté.");

    public static async Task<ReaderState> ReaderStateAsync(this AppDbContext db, ClaimsPrincipal user)
    {
        if (user.UserIdOrNull() is not { } userId)
        {
            return ReaderState.Anonymous;
        }

        var hideSpoilers = await db.Users.Where(u => u.Id == userId).Select(u => (bool?)u.HideSpoilers).FirstOrDefaultAsync();
        if (hideSpoilers is null)
        {
            return ReaderState.Anonymous; // compte supprimé entre-temps
        }

        var read = await db.ReadVolumes.Where(v => v.UserId == userId).Select(v => v.Number).ToListAsync();
        return new ReaderState(true, userId, hideSpoilers.Value, read.ToHashSet());
    }

    /// <summary>Note moyenne et nombre d'avis pour chaque arc noté.</summary>
    public static async Task<Dictionary<string, ArcRating>> ArcRatingsAsync(this AppDbContext db) =>
        (await db.ArcReviews
            .GroupBy(r => r.ArcSlug)
            .Select(g => new { Slug = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync())
        .ToDictionary(r => r.Slug, r => new ArcRating(r.Slug, r.Average, r.Count));

    public static IQueryable<ReviewView> ReviewViews(this IQueryable<ArcReview> reviews) =>
        reviews.Select(r => new ReviewView(r.Id, r.ArcSlug, r.User.DisplayName, r.UserId, r.Rating, r.Text, r.UpdatedAt));

    /// <summary>Nombre de lecteurs qui ont mis chaque personnage en favori.</summary>
    public static async Task<Dictionary<string, int>> FavoriteCountsAsync(this AppDbContext db) =>
        await db.FavoriteCharacters
            .GroupBy(f => f.CharacterSlug)
            .Select(g => new { Slug = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Slug, g => g.Count);
}
