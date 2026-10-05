namespace TaskFlow.Models;

public static class TagList
{
    public const int MaxTags = 5;
    public const int MaxTagLength = 20;

    /// <summary>
    /// "Docker, #infra ,docker" -> ["docker", "infra"] : minuscules, sans '#', sans doublons, 5 maximum.
    /// </summary>
    public static IReadOnlyList<string> Parse(string? raw) =>
        (raw ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.TrimStart('#').Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Select(t => t.Length > MaxTagLength ? t[..MaxTagLength] : t)
            .Distinct()
            .Take(MaxTags)
            .ToList();

    public static string Normalize(string? raw) => string.Join(",", Parse(raw));

    /// <summary>Format affiché dans les champs de saisie.</summary>
    public static string ToInput(string stored) => string.Join(", ", Parse(stored));
}
