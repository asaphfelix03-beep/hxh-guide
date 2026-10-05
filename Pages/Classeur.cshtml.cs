using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages;

/// <summary>Le classeur : un emplacement par tome, coché quand le tome est lu.</summary>
public class ClasseurModel(AppDbContext db) : PageModel
{
    public ReaderState State { get; private set; } = ReaderState.Anonymous;

    public async Task OnGetAsync() => State = await db.ReaderStateAsync(User);

    /// <summary>Coche ou décoche un tome.</summary>
    public async Task<IActionResult> OnPostToggleAsync(int tome)
    {
        if (tome is < 1 or > Catalog.TotalVolumes)
        {
            return BadRequest();
        }

        var userId = User.UserId();
        var removed = await db.ReadVolumes.Where(v => v.UserId == userId && v.Number == tome).ExecuteDeleteAsync();
        if (removed == 0)
        {
            db.ReadVolumes.Add(new ReadVolume { UserId = userId, Number = tome });
            await db.SaveChangesAsync();
        }
        return Redirect($"/Classeur#tome-{tome}");
    }

    /// <summary>Marque comme lus tous les tomes jusqu'à <paramref name="jusqua"/> (et seulement ceux-là).</summary>
    public async Task<IActionResult> OnPostUpToAsync(int jusqua)
    {
        if (jusqua is < 0 or > Catalog.TotalVolumes)
        {
            TempData["Error"] = $"Choisis un tome entre 1 et {Catalog.TotalVolumes}.";
            return RedirectToPage();
        }

        var userId = User.UserId();
        await db.ReadVolumes.Where(v => v.UserId == userId).ExecuteDeleteAsync();
        db.ReadVolumes.AddRange(Enumerable.Range(1, jusqua).Select(n => new ReadVolume { UserId = userId, Number = n }));
        await db.SaveChangesAsync();

        TempData["Message"] = jusqua == 0 ? "Classeur vidé." : $"Tomes 1 à {jusqua} marqués comme lus.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSpoilersAsync(bool masquer)
    {
        var userId = User.UserId();
        await db.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(u => u.HideSpoilers, masquer));
        TempData["Message"] = masquer
            ? "Mode sans spoiler activé : le contenu des arcs que tu n'as pas atteints est masqué."
            : "Mode sans spoiler désactivé : tout le guide s'affiche.";
        return RedirectToPage();
    }
}
