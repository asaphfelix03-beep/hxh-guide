using System.ComponentModel.DataAnnotations;
using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Pages.Arcs;

public class DetailModel(AppDbContext db) : PageModel
{
    public Arc Arc { get; private set; } = null!;
    public ReaderState State { get; private set; } = ReaderState.Anonymous;
    public ArcRating? Rating { get; private set; }
    public List<ReviewView> Reviews { get; private set; } = [];
    public List<CharacterCardModel> Characters { get; private set; } = [];
    public bool HasMyReview { get; private set; }

    [BindProperty]
    public ReviewInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (!await LoadAsync(slug))
        {
            return NotFound();
        }
        return Page();
    }

    /// <summary>Crée ou met à jour l'avis du lecteur sur cet arc.</summary>
    public async Task<IActionResult> OnPostReviewAsync(string slug)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }
        if (!await LoadAsync(slug))
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.UserId();
        var review = await db.ArcReviews.FirstOrDefaultAsync(r => r.UserId == userId && r.ArcSlug == Arc.Slug);
        if (review is null)
        {
            review = new ArcReview { UserId = userId, ArcSlug = Arc.Slug };
            db.ArcReviews.Add(review);
        }
        review.Rating = Input.Rating;
        review.Text = string.IsNullOrWhiteSpace(Input.Text) ? null : Input.Text.Trim();
        review.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        TempData["Message"] = "Avis enregistré. Merci !";
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostDeleteReviewAsync(string slug)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var userId = User.UserId();
        await db.ArcReviews.Where(r => r.UserId == userId && r.ArcSlug == slug).ExecuteDeleteAsync();
        TempData["Message"] = "Avis supprimé.";
        return RedirectToPage(new { slug });
    }

    private async Task<bool> LoadAsync(string slug)
    {
        if (Catalog.ArcBySlug(slug) is not { } arc)
        {
            return false;
        }

        Arc = arc;
        State = await db.ReaderStateAsync(User);
        Rating = (await db.ArcRatingsAsync()).GetValueOrDefault(arc.Slug);
        Reviews = await db.ArcReviews.Where(r => r.ArcSlug == arc.Slug)
            .OrderByDescending(r => r.UpdatedAt)
            .ReviewViews()
            .ToListAsync();

        var favorites = await db.FavoriteCountsAsync();
        var mine = State.UserId is { } id
            ? (await db.FavoriteCharacters.Where(f => f.UserId == id).Select(f => f.CharacterSlug).ToListAsync()).ToHashSet()
            : [];
        Characters = Catalog.CharactersOf(arc)
            .Select(c => new CharacterCardModel(c, State.CanSee(c) || !State.SignedIn, mine.Contains(c.Slug), favorites.GetValueOrDefault(c.Slug)))
            .ToList();

        if (State.UserId is { } userId && Reviews.FirstOrDefault(r => r.AuthorId == userId) is { } myReview)
        {
            HasMyReview = true;
            if (Request.Method == HttpMethods.Get)
            {
                Input = new ReviewInput { Rating = myReview.Rating, Text = myReview.Text };
            }
        }
        return true;
    }

    public class ReviewInput
    {
        [Display(Name = "Ta note")]
        [Range(1, 5, ErrorMessage = "Choisis une note de 1 à 5 étoiles.")]
        public int Rating { get; set; }

        [Display(Name = "Ton avis (facultatif)")]
        [StringLength(1000, ErrorMessage = "1000 caractères maximum.")]
        public string? Text { get; set; }
    }
}
