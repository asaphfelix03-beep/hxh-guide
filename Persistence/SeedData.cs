using HxhGuide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Persistence;

/// <summary>
/// Lecteurs de démonstration (activés par Seed__Demo=true) : progressions, avis et favoris variés,
/// pour essayer le guide sans s'inscrire. Mot de passe commun : <see cref="DemoPassword"/>.
/// </summary>
public static class SeedData
{
    public const string DemoEmail = "demo@guide.local";
    public const string DemoPassword = "Demo1234";

    public static async Task SeedDemoAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var users = services.GetRequiredService<UserManager<Reader>>();
        var alex = await CreateReaderAsync(users, DemoEmail, "Alex");
        var camille = await CreateReaderAsync(users, "camille@guide.local", "Camille");
        var bilal = await CreateReaderAsync(users, "bilal@guide.local", "Bilal");
        var sofia = await CreateReaderAsync(users, "sofia@guide.local", "Sofia");

        void ReadUpTo(Reader reader, int last) =>
            db.ReadVolumes.AddRange(Enumerable.Range(1, last).Select(n => new ReadVolume { UserId = reader.Id, Number = n }));
        ReadUpTo(alex, 24);
        ReadUpTo(camille, Catalog.TotalVolumes);
        ReadUpTo(bilal, 30);
        ReadUpTo(sofia, 13);

        void Review(Reader reader, string arc, int rating, string text) =>
            db.ArcReviews.Add(new ArcReview { UserId = reader.Id, ArcSlug = arc, Rating = rating, Text = text });

        Review(camille, "examen-hunter", 5, "Le départ idéal : chaque épreuve présente un personnage, et le quatuor se forme naturellement.");
        Review(bilal, "examen-hunter", 4, "Classique mais efficace. Le marathon dans le tunnel reste culte.");
        Review(alex, "examen-hunter", 5, "J'ai commencé pour Gon, je suis resté pour Killua.");
        Review(camille, "famille-zoldyck", 4, "Court mais marquant : on comprend enfin d'où vient Killua.");
        Review(bilal, "famille-zoldyck", 3, "Sympa, mais c'est surtout une transition.");
        Review(camille, "tour-celeste", 4, "L'explication du Nen la plus claire que j'aie lue dans un shōnen.");
        Review(sofia, "tour-celeste", 4, "Le premier vrai combat contre Hisoka donne le ton pour la suite.");
        Review(alex, "tour-celeste", 4, "La divination par l'eau pour expliquer les types de Nen : génial.");
        Review(camille, "york-shin", 5, "Kurapika porte l'arc sur ses épaules. La Brigade fantôme est fascinante.");
        Review(bilal, "york-shin", 5, "Un vrai thriller : on change de camp à chaque chapitre.");
        Review(sofia, "york-shin", 5, "Mon arc préféré, sans hésiter.");
        Review(alex, "york-shin", 5, "La tension pendant les enchères, les chaînes de Kurapika… parfait.");
        Review(camille, "greed-island", 4, "Les règles du jeu sont d'une intelligence rare.");
        Review(bilal, "greed-island", 4, "La balle au prisonnier contre Razor, quel moment.");
        Review(alex, "greed-island", 4, "L'entraînement avec Biscuit rend la progression de Gon et Killua crédible.");
        Review(camille, "fourmis-chimeres", 5, "Long, dense, bouleversant. La fin de l'arc est un chef-d'œuvre.");
        Review(bilal, "fourmis-chimeres", 5, "On n'en ressort pas indemne.");
        Review(camille, "election", 4, "Un mélange étonnant de manœuvres politiques et d'émotion familiale.");
        Review(bilal, "election", 4, "Plus calme, mais très malin.");
        Review(camille, "continent-noir", 4, "Presque un autre manga : il faut s'accrocher, mais c'est passionnant.");

        void Favorite(Reader reader, params string[] slugs) =>
            db.FavoriteCharacters.AddRange(slugs.Select(s => new FavoriteCharacter { UserId = reader.Id, CharacterSlug = s }));
        Favorite(alex, "killua-zoldyck", "kurapika", "isaac-netero");
        Favorite(camille, "kurapika", "meruem", "komugi");
        Favorite(bilal, "killua-zoldyck", "hisoka", "knuckle-bine");
        Favorite(sofia, "kurapika", "chrollo-lucilfer");

        await db.SaveChangesAsync();
    }

    private static async Task<Reader> CreateReaderAsync(UserManager<Reader> users, string email, string name)
    {
        var reader = new Reader { UserName = email, Email = email, DisplayName = name, EmailConfirmed = true };
        var result = await users.CreateAsync(reader, DemoPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
        return reader;
    }
}
