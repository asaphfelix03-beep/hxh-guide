namespace HxhGuide.Models;

/// <summary>
/// Portraits facultatifs des personnages : wwwroot/images/personnages/{slug}.webp (ou .jpg, .png, .avif).
/// Le dossier est lu une fois au démarrage ; un personnage sans fichier garde son avatar à initiales.
/// </summary>
public class CharacterImages(IWebHostEnvironment env)
{
    public const string Folder = "images/personnages";
    private static readonly string[] Extensions = [".webp", ".avif", ".jpg", ".jpeg", ".png"];

    private readonly Lazy<Dictionary<string, string>> urls = new(() =>
        env.WebRootFileProvider.GetDirectoryContents(Folder)
            .Where(file => !file.IsDirectory && Extensions.Contains(Path.GetExtension(file.Name).ToLowerInvariant()))
            .GroupBy(file => Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant())
            .ToDictionary(group => group.Key, group => $"/{Folder}/{group.First().Name}"));

    public string? UrlFor(string slug) => urls.Value.GetValueOrDefault(slug);
}
