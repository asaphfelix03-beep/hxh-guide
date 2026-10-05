using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages.Personnages;

public class IndexModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "nen")]
    public NenType? Nen { get; set; }

    [BindProperty(SupportsGet = true, Name = "arc")]
    public string? ArcSlug { get; set; }

    [BindProperty(SupportsGet = true, Name = "favoris")]
    public bool OnlyFavorites { get; set; }

    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public List<CharacterCardModel> Cards { get; private set; } = [];

    public async Task OnGetAsync()
    {
        State = await db.ReaderStateAsync(User);
        var counts = await db.FavoriteCountsAsync();
        var mine = State.UserId is { } id
            ? (await db.FavoriteCharacters.Where(f => f.UserId == id).Select(f => f.CharacterSlug).ToListAsync()).ToHashSet()
            : [];

        Cards = Catalog.Characters
            .Where(c => Nen is null || c.Nen == Nen)
            .Where(c => string.IsNullOrEmpty(ArcSlug) || c.FirstArcSlug == ArcSlug)
            .Where(c => !OnlyFavorites || mine.Contains(c.Slug))
            .Select(c => new CharacterCardModel(c, State.CanSee(c) || !State.SignedIn, mine.Contains(c.Slug), counts.GetValueOrDefault(c.Slug)))
            .ToList();
    }
}
