namespace HxhGuide.Models;

/// <summary>Note moyenne d'un arc par la communauté.</summary>
public record ArcRating(string ArcSlug, double Average, int Count);

/// <summary>Un avis tel qu'affiché (avec le nom de son auteur).</summary>
public record ReviewView(int Id, string ArcSlug, string Author, string AuthorId, int Rating, string? Text, DateTime UpdatedAt);

public static class Initials
{
    public static string Of(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}",
        };
    }
}
