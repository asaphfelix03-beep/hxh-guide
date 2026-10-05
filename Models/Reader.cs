using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HxhGuide.Models;

/// <summary>Un lecteur du guide : son compte et ses préférences.</summary>
public class Reader : IdentityUser
{
    public string DisplayName { get; set; } = "";

    /// <summary>Masque le contenu des arcs pas encore atteints (activé par défaut).</summary>
    public bool HideSpoilers { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Un tome lu : une carte dans le classeur du lecteur.</summary>
public class ReadVolume
{
    public string UserId { get; set; } = "";
    public Reader User { get; set; } = null!;

    [Range(1, Catalog.TotalVolumes)]
    public int Number { get; set; }

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Avis d'un lecteur sur un arc (un seul par arc et par lecteur).</summary>
public class ArcReview
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public Reader User { get; set; } = null!;

    [MaxLength(40)]
    public string ArcSlug { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Text { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class FavoriteCharacter
{
    public string UserId { get; set; } = "";
    public Reader User { get; set; } = null!;

    [MaxLength(40)]
    public string CharacterSlug { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
