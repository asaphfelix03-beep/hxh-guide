using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HxhGuide.Pages.Arcs;

public class IndexModel(AppDbContext db) : PageModel
{
    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public Dictionary<string, ArcRating> Ratings { get; private set; } = [];

    public async Task OnGetAsync()
    {
        State = await db.ReaderStateAsync(User);
        Ratings = await db.ArcRatingsAsync();
    }
}
