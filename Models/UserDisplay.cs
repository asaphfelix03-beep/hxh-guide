namespace TaskFlow.Models;

/// <summary>Aides d'affichage : initiales et couleur stable pour les avatars et les étiquettes.</summary>
public static class UserDisplay
{
    private const int PaletteSize = 8;

    public static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}",
        };
    }

    /// <summary>Numéro de couleur (0-7) toujours identique pour un même texte : classes CSS .tone-0 à .tone-7.</summary>
    public static int Tone(string text)
    {
        var hash = 0;
        foreach (var c in text)
        {
            hash = unchecked(hash * 31 + c);
        }
        return Math.Abs(hash % PaletteSize);
    }
}
