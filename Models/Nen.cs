namespace HxhGuide.Models;

/// <summary>
/// Les six types de Nen, dans l'ordre du diagramme hexagonal (sens horaire).
/// L'ordre sert au calcul des affinités : deux types voisins sur l'hexagone sont proches.
/// </summary>
public enum NenType
{
    Renforcement = 0,
    Transformation = 1,
    Materialisation = 2,
    Specialisation = 3,
    Manipulation = 4,
    Emission = 5,
}

public record NenInfo(
    NenType Type,
    string Name,
    string Slug,
    string Power,
    string Personality,
    string WaterSign,
    IReadOnlyList<string> Characters);

public static class NenTypes
{
    public static readonly IReadOnlyList<NenInfo> All =
    [
        new(NenType.Renforcement, "Renforcement", "renforcement",
            "Décuple la force du corps et la solidité des objets. Le type le plus équilibré pour le combat.",
            "Simple et déterminé, tu fonces droit au but et tu dis ce que tu penses.",
            "Le volume d'eau augmente jusqu'à déborder.",
            ["Gon Freecss", "Isaac Netero", "Uvogin"]),
        new(NenType.Transformation, "Transformation", "transformation",
            "Donne à l'aura les propriétés d'autre chose : électricité, élasticité, tranchant…",
            "Imprévisible et malin, tu aimes surprendre et tu caches bien ton jeu.",
            "Le goût de l'eau change.",
            ["Killua Zoldyck", "Hisoka", "Biscuit Krueger"]),
        new(NenType.Materialisation, "Matérialisation", "materialisation",
            "Crée des objets concrets à partir de l'aura, avec des règles précises.",
            "Prudent et réfléchi, tu restes sur tes gardes et tu tiens tes promesses.",
            "Des impuretés apparaissent dans l'eau.",
            ["Kurapika", "Kortopi", "Shizuku"]),
        new(NenType.Specialisation, "Spécialisation", "specialisation",
            "Des capacités uniques, impossibles à classer dans les autres types.",
            "Indépendant et charismatique, tu suis ta propre voie.",
            "Un changement imprévisible se produit.",
            ["Chrollo Lucilfer", "Neon Nostrade", "Pariston Hill"]),
        new(NenType.Manipulation, "Manipulation", "manipulation",
            "Contrôle des objets ou des êtres vivants à distance.",
            "Logique et méthodique, tu avances toujours selon ton propre plan.",
            "La feuille se met à bouger à la surface.",
            ["Illumi Zoldyck", "Shalnark", "Morel"]),
        new(NenType.Emission, "Émission", "emission",
            "Projette l'aura loin du corps, sans qu'elle s'affaiblisse.",
            "Impulsif et chaleureux, tu t'emportes vite… et tu oublies aussi vite.",
            "La couleur de l'eau change.",
            ["Leorio Paladiknight", "Razor", "Franklin"]),
    ];

    public static NenInfo Info(this NenType type) => All[(int)type];

    /// <summary>
    /// Pourcentage de maîtrise d'un autre type : 100 % pour le sien, puis -20 % par case
    /// d'écart sur l'hexagone (80 % pour les voisins, 40 % pour le type opposé).
    /// </summary>
    public static int Affinity(this NenType own, NenType other)
    {
        var distance = Math.Abs((int)own - (int)other);
        distance = Math.Min(distance, All.Count - distance);
        return 100 - 20 * distance;
    }
}
