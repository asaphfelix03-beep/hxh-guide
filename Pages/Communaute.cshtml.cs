using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages;

public class CommunauteModel(AppDbContext db) : PageModel
{
    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public List<(Arc Arc, ArcRating Rating)> TopArcs { get; private set; } = [];
    public List<(Character Character, int Count)> TopCharacters { get; private set; } = [];
    public List<ReviewView> Latest { get; private set; } = [];
    public int ReaderCount { get; private set; }

    public async Task OnGetAsync()
    {
        State = await db.ReaderStateAsync(User);
        ReaderCount = await db.Users.CountAsync();

        var ratings = await db.ArcRatingsAsync();
        TopArcs = Catalog.Arcs
            .Where(a => ratings.ContainsKey(a.Slug))
            .Select(a => (a, ratings[a.Slug]))
            .OrderByDescending(x => x.Item2.Average)
            .ThenByDescending(x => x.Item2.Count)
            .ToList();

        var favorites = await db.FavoriteCountsAsync();
        TopCharacters = favorites
            .Select(f => (Catalog.CharacterBySlug(f.Key), f.Value))
            .Where(x => x.Item1 is not null)
            .Select(x => (x.Item1!, x.Value))
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Item1.Name)
            .Take(8)
            .ToList();

        Latest = await db.ArcReviews.OrderByDescending(r => r.UpdatedAt).Take(6).ReviewViews().ToListAsync();
    }
}
