using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages;

public class IndexModel(AppDbContext db, IConfiguration configuration) : PageModel
{
    public bool DemoEnabled => configuration.GetValue<bool>("Seed:Demo");

    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public Dictionary<string, ArcRating> Ratings { get; private set; } = [];
    public List<Character> Favorites { get; private set; } = [];
    public int ReaderCount { get; private set; }

    public async Task OnGetAsync()
    {
        State = await db.ReaderStateAsync(User);
        Ratings = await db.ArcRatingsAsync();

        if (!State.SignedIn)
        {
            ReaderCount = await db.Users.CountAsync();
            return;
        }

        var slugs = await db.FavoriteCharacters.Where(f => f.UserId == State.UserId)
            .OrderBy(f => f.CreatedAt).Select(f => f.CharacterSlug).ToListAsync();
        Favorites = slugs.Select(Catalog.CharacterBySlug).OfType<Character>().ToList();
    }
}
