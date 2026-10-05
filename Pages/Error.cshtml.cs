using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskFlow.Pages;

// La page d'erreur peut être ré-exécutée après n'importe quelle requête, y compris un POST.
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? Code { get; set; }

    public (string Title, string Message) Text => Code switch
    {
        404 => ("Page introuvable", "Cette page n'existe pas, ou vous n'avez pas accès à ce projet."),
        403 => ("Accès refusé", "Vous n'avez pas les droits pour cette action."),
        400 => ("Requête invalide", "La requête n'a pas pu être traitée. Rechargez la page et réessayez."),
        _ => ("Une erreur est survenue", "Réessayez dans un instant. Si le problème continue : docker compose logs app."),
    };
}
