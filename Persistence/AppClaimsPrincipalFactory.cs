using System.Security.Claims;
using HxhGuide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HxhGuide.Persistence;

/// <summary>Ajoute le nom affiché au cookie de session : affiché partout sans requête en base.</summary>
public class AppClaimsPrincipalFactory(
    UserManager<Reader> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<Reader, IdentityRole>(userManager, roleManager, options)
{
    public const string DisplayNameClaim = "display_name";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Reader user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
        return identity;
    }
}

public static class ReaderClaims
{
    public static string DisplayName(this ClaimsPrincipal user) =>
        user.FindFirstValue(AppClaimsPrincipalFactory.DisplayNameClaim) ?? user.Identity?.Name ?? "";
}
