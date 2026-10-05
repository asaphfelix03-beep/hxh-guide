using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = "";

    public string Initials => UserDisplay.Initials(DisplayName);
}
