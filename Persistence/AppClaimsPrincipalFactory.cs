using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TaskFlow.Models;

namespace TaskFlow.Persistence;

/// <summary>Ajoute le nom affiché au cookie de session pour l'afficher sans requête en base.</summary>
public class AppClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, options)
{
    public const string DisplayNameClaim = "display_name";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
        return identity;
    }
}
