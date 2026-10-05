using HxhGuide.Models;
using HxhGuide.Persistence;

namespace HxhGuide.Pages;

/// <summary>Données de la carte d'un personnage (Pages/Shared/_CharacterCard.cshtml).</summary>
public record CharacterCardModel(Character Character, bool Visible, bool IsFavorite, int FavoriteCount);

/// <summary>Données du classeur de tomes (Pages/Shared/_Binder.cshtml).</summary>
public record BinderModel(ReaderState State, bool Interactive, bool Compact = false);
