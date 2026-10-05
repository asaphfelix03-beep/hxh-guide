namespace HxhGuide.Models;

/// <summary>Un arc du manga. Les numéros de chapitres sont approximatifs (les arcs se chevauchent parfois).</summary>
public record Arc(
    string Slug,
    string Title,
    int FirstVolume,
    int LastVolume,
    int FirstChapter,
    int? LastChapter,
    string Summary,
    bool Ongoing = false)
{
    public int Order => Catalog.Arcs.ToList().IndexOf(this) + 1;
    public int VolumeCount => LastVolume - FirstVolume + 1;
    public string Volumes => FirstVolume == LastVolume ? $"Tome {FirstVolume}" : $"Tomes {FirstVolume} à {LastVolume}{(Ongoing ? "+" : "")}";
    public string Chapters => LastChapter is { } last ? $"Chapitres {FirstChapter} à {last} env." : $"Chapitres {FirstChapter} et suivants";
    public bool Contains(int volume) => volume >= FirstVolume && volume <= LastVolume;
}

public record Character(
    string Slug,
    string Name,
    NenType? Nen,
    string Affiliation,
    string FirstArcSlug,
    string Description,
    string? Ability = null)
{
    public Arc FirstArc => Catalog.ArcBySlug(FirstArcSlug)!;
}

/// <summary>
/// Le contenu du guide : arcs, tomes et personnages du manga Hunter × Hunter de Yoshihiro Togashi.
/// Textes originaux, écrits pour ce projet. Pour ajouter un tome paru, augmenter <see cref="TotalVolumes"/>
/// et le dernier tome de l'arc en cours.
/// </summary>
public static class Catalog
{
    public const int TotalVolumes = 38;

    public static readonly IReadOnlyList<Arc> Arcs =
    [
        new("examen-hunter", "L'examen Hunter", 1, 5, 1, 36,
            "Gon, douze ans, quitte l'île de la Baleine pour devenir Hunter, comme ce père qu'il n'a jamais connu. "
            + "Avec Kurapika, Leorio et Killua, il enchaîne des épreuves aussi absurdes que mortelles."),
        new("famille-zoldyck", "La famille Zoldyck", 5, 5, 37, 44,
            "Killua est rentré chez lui, dans le domaine d'une famille d'assassins légendaires. "
            + "Gon et ses amis décident d'aller le chercher, quitte à forcer la porte… littéralement."),
        new("tour-celeste", "La Tour céleste", 5, 7, 45, 63,
            "Pour s'endurcir et gagner de l'argent, Gon et Killua grimpent les étages d'une arène de combat géante. "
            + "Ils y découvrent le Nen, l'art de maîtriser son aura."),
        new("york-shin", "York Shin", 8, 13, 64, 119,
            "La grande vente aux enchères de York Shin attire la pègre du monde entier, et la Brigade fantôme. "
            + "Kurapika n'attendait que ça : retrouver ceux qui ont massacré son clan."),
        new("greed-island", "Greed Island", 13, 18, 120, 185,
            "Un jeu vidéo introuvable, créé par des Hunters, où l'on peut mourir pour de vrai. "
            + "Gon y entre pour suivre la piste de son père et s'entraîne sans relâche auprès de Biscuit."),
        new("fourmis-chimeres", "Les Fourmis-chimères", 18, 30, 186, 318,
            "Une reine fourmi géante s'échoue sur le continent et donne naissance à une espèce qui dévore les humains. "
            + "L'arc le plus long, le plus sombre et souvent le plus aimé du manga."),
        new("election", "L'élection du président", 30, 32, 319, 339,
            "L'Association Hunter doit élire son nouveau président. Pendant que les candidats manœuvrent, "
            + "Killua tente l'impossible grâce à un secret bien gardé de sa famille."),
        new("continent-noir", "Le Continent noir", 32, TotalVolumes, 340, null,
            "Une expédition part vers le monde inconnu au-delà de la mer. À bord du navire, quatorze princes "
            + "doivent s'éliminer pour hériter du trône, et Kurapika protège l'un d'eux. L'arc est toujours en cours.",
            Ongoing: true),
    ];

    public static readonly IReadOnlyList<Character> Characters =
    [
        new("gon-freecss", "Gon Freecss", NenType.Renforcement, "Hunter", "examen-hunter",
            "Enfant de l'île de la Baleine, parti sur les traces de son père. Instinctif, têtu, d'une loyauté absolue envers ses amis.",
            "Jajanken"),
        new("killua-zoldyck", "Killua Zoldyck", NenType.Transformation, "Famille Zoldyck", "examen-hunter",
            "Héritier d'une famille d'assassins, il a fui pour vivre sa propre vie. Le meilleur ami de Gon.",
            "Aura électrique"),
        new("kurapika", "Kurapika", NenType.Materialisation, "Clan Kurta", "examen-hunter",
            "Dernier survivant du clan Kurta, dont les yeux deviennent écarlates sous l'émotion. Il vit pour sa vengeance.",
            "Les chaînes"),
        new("leorio", "Leorio Paladiknight", NenType.Emission, "Hunter", "examen-hunter",
            "Veut devenir médecin pour soigner gratuitement ceux qui n'en ont pas les moyens. Grande gueule au grand cœur.",
            "Coup de poing à distance"),
        new("hisoka", "Hisoka", NenType.Transformation, "Indépendant", "examen-hunter",
            "Magicien imprévisible qui ne vit que pour affronter des adversaires puissants… quand ils sont « mûrs ».",
            "Bungee Gum"),
        new("illumi-zoldyck", "Illumi Zoldyck", NenType.Manipulation, "Famille Zoldyck", "examen-hunter",
            "Frère aîné de Killua, assassin glacial qui contrôle ses cibles avec des aiguilles.",
            "Aiguilles"),
        new("isaac-netero", "Isaac Netero", NenType.Renforcement, "Association Hunter", "examen-hunter",
            "Président de l'Association Hunter. Un vieil homme farceur, et l'un des combattants les plus forts au monde.",
            "Hyakushiki Kannon"),
        new("kite", "Kite", NenType.Materialisation, "Hunter", "examen-hunter",
            "Disciple de Ging. C'est lui qui a sauvé Gon enfant et lui a donné envie de devenir Hunter.",
            "Crazy Slots"),
        new("chrollo-lucilfer", "Chrollo Lucilfer", NenType.Specialisation, "Brigade fantôme", "york-shin",
            "Chef de la Brigade fantôme, calme et cultivé, capable de voler les capacités des autres.",
            "Le livre du voleur"),
        new("uvogin", "Uvogin", NenType.Renforcement, "Brigade fantôme", "york-shin",
            "Le plus fort physiquement de la Brigade. Il ne compte que sur ses poings.",
            "Big Bang Impact"),
        new("shalnark", "Shalnark", NenType.Manipulation, "Brigade fantôme", "york-shin",
            "Le stratège de la Brigade, qui pirate les esprits aussi bien que les ordinateurs."),
        new("biscuit-krueger", "Biscuit Krueger", NenType.Transformation, "Hunter", "greed-island",
            "Hunter chevronnée à l'apparence d'une fillette. Elle devient la maîtresse d'armes de Gon et Killua.",
            "Cookie-chan"),
        new("razor", "Razor", NenType.Emission, "Greed Island", "greed-island",
            "Gardien du jeu, il défie Gon et ses amis dans une partie de balle au prisonnier inoubliable.",
            "Les 14 démons"),
        new("knuckle-bine", "Knuckle Bine", NenType.Emission, "Hunter", "fourmis-chimeres",
            "Air de voyou, cœur tendre : il préfère neutraliser ses adversaires plutôt que les éliminer.",
            "Hakoware"),
        new("morel", "Morel Mackernasey", NenType.Manipulation, "Hunter", "fourmis-chimeres",
            "Hunter vétéran, maître de la fumée et mentor de Knuckle.",
            "Deep Purple"),
        new("neferpitou", "Neferpitou", NenType.Specialisation, "Fourmis-chimères", "fourmis-chimeres",
            "Garde royal du Roi, d'une puissance et d'une curiosité inquiétantes.",
            "Docteur Blythe"),
        new("meruem", "Meruem", null, "Fourmis-chimères", "fourmis-chimeres",
            "Le Roi des fourmis-chimères, né pour dominer le monde… jusqu'à ce qu'une partie de Gungi change tout."),
        new("komugi", "Komugi", null, "Championne de Gungi", "fourmis-chimeres",
            "Jeune joueuse aveugle, invaincue au Gungi, un jeu de stratégie redoutable."),
        new("ging-freecss", "Ging Freecss", null, "Zodiaques", "election",
            "Le père de Gon. Hunter de génie, insaisissable, aussi doué qu'irresponsable."),
        new("alluka-zoldyck", "Alluka Zoldyck", null, "Famille Zoldyck", "election",
            "Membre caché de la famille Zoldyck, au pouvoir aussi immense que mystérieux."),
        new("pariston-hill", "Pariston Hill", null, "Association Hunter", "election",
            "Vice-président de l'Association, sourire éternel et goût prononcé pour le chaos."),
    ];

    public static Arc? ArcBySlug(string? slug) => Arcs.FirstOrDefault(a => a.Slug == slug);

    public static Character? CharacterBySlug(string? slug) => Characters.FirstOrDefault(c => c.Slug == slug);

    /// <summary>Arcs couverts par un tome (un tome peut contenir la fin d'un arc et le début du suivant).</summary>
    public static IEnumerable<Arc> ArcsOfVolume(int volume) => Arcs.Where(a => a.Contains(volume));

    /// <summary>Arc principal d'un tome : le dernier qui y commence ou s'y poursuit.</summary>
    public static Arc MainArcOfVolume(int volume) => ArcsOfVolume(volume).Last();

    public static IEnumerable<Character> CharactersOf(Arc arc) => Characters.Where(c => c.FirstArcSlug == arc.Slug);
}
