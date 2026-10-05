using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HxhGuide.Pages;

// La page d'erreur peut être ré-exécutée après n'importe quelle requête, y compris un POST.
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? Code { get; set; }

    public (string Title, string Message) Text => Code switch
    {
        404 => ("Introuvable", "Cette page n'existe pas. Vérifie l'adresse, ou repars de l'accueil."),
        403 => ("Accès refusé", "Tu n'as pas accès à cette page."),
        400 => ("Requête invalide", "La page a expiré. Recharge-la puis réessaie."),
        _ => ("Une erreur est survenue", "Réessaie dans un instant."),
    };
}
