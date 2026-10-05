using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages.Personnages;

public class DetailModel(AppDbContext db) : PageModel
{
    public Character Character { get; private set; } = null!;
    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public bool IsFavorite { get; private set; }
    public int FavoriteCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (Catalog.CharacterBySlug(slug) is not { } character)
        {
            return NotFound();
        }

        Character = character;
        State = await db.ReaderStateAsync(User);
        FavoriteCount = await db.FavoriteCharacters.CountAsync(f => f.CharacterSlug == slug);
        IsFavorite = State.UserId is { } userId
            && await db.FavoriteCharacters.AnyAsync(f => f.UserId == userId && f.CharacterSlug == slug);
        return Page();
    }

    /// <summary>Ajoute ou retire le personnage des favoris du lecteur.</summary>
    public async Task<IActionResult> OnPostFavoriteAsync(string slug)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }
        if (Catalog.CharacterBySlug(slug) is not { } character)
        {
            return NotFound();
        }

        var userId = User.UserId();
        var removed = await db.FavoriteCharacters.Where(f => f.UserId == userId && f.CharacterSlug == slug).ExecuteDeleteAsync();
        if (removed == 0)
        {
            db.FavoriteCharacters.Add(new FavoriteCharacter { UserId = userId, CharacterSlug = slug });
            await db.SaveChangesAsync();
            TempData["Message"] = $"{character.Name} ajouté à tes favoris.";
        }
        else
        {
            TempData["Message"] = $"{character.Name} retiré de tes favoris.";
        }
        return RedirectToPage(new { slug });
    }
}
