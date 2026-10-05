using System.ComponentModel.DataAnnotations;
using HxhGuide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HxhGuide.Pages.Compte;

public class ConnexionModel(SignInManager<Reader> signIn, IConfiguration configuration) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true, Name = "retour")]
    public string? ReturnUrl { get; set; }

    public bool DemoEnabled => configuration.GetValue<bool>("Seed:Demo");

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Redirect("/") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signIn.PasswordSignInAsync(Input.Email.Trim(), Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            // N'accepte que les adresses locales : empêche une redirection vers un site malveillant.
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Compte temporairement bloqué après plusieurs échecs. Réessaie dans quelques minutes."
            : "Adresse e-mail ou mot de passe incorrect.");
        return Page();
    }

    public class LoginInput
    {
        [Display(Name = "Adresse e-mail")]
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        public string Email { get; set; } = "";

        [Display(Name = "Mot de passe")]
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Rester connecté")]
        public bool RememberMe { get; set; } = true;
    }
}
