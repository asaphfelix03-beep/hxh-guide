using System.ComponentModel.DataAnnotations;
using HxhGuide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HxhGuide.Pages.Compte;

public class InscriptionModel(UserManager<Reader> users, SignInManager<Reader> signIn) : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Redirect("/") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new Reader
        {
            UserName = Input.Email.Trim(),
            Email = Input.Email.Trim(),
            DisplayName = Input.DisplayName.Trim(),
        };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await signIn.SignInAsync(user, isPersistent: true);
        TempData["Message"] = $"Bienvenue, {user.DisplayName} ! Coche les tomes que tu as déjà lus pour commencer ton classeur.";
        return Redirect("/Classeur");
    }

    public class RegisterInput
    {
        [Display(Name = "Pseudo")]
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(60, ErrorMessage = "60 caractères maximum.")]
        public string DisplayName { get; set; } = "";

        [Display(Name = "Adresse e-mail")]
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        public string Email { get; set; } = "";

        [Display(Name = "Mot de passe")]
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Confirmer le mot de passe")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Les deux mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
