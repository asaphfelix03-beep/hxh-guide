using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HxhGuide.Pages.Nen;

/// <summary>Le guide du Nen : les six types, l'hexagone et la divination par l'eau.</summary>
public class IndexModel(AppDbContext db) : PageModel
{
    public ReaderState State { get; private set; } = ReaderState.Anonymous;

    public async Task OnGetAsync() => State = await db.ReaderStateAsync(User);

    /// <summary>Personnages du catalogue de ce type, sans ceux que le lecteur ne doit pas encore voir.</summary>
    public IEnumerable<Character> CharactersOf(NenType type) =>
        Catalog.Characters.Where(c => c.Nen == type && (State.CanSee(c) || !State.SignedIn));
}
