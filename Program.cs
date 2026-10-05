using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using HxhGuide.Endpoints;
using HxhGuide.Models;
using HxhGuide.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// Chaîne de connexion PostgreSQL : variable d'environnement ConnectionStrings__Default (voir docker-compose).
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("La chaîne de connexion 'ConnectionStrings:Default' est manquante.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

builder.Services.AddIdentity<Reader, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<FrenchIdentityErrorDescriber>()
    .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "guide.session";
    options.LoginPath = "/Compte/Connexion";
    options.LogoutPath = "/Compte/Deconnexion";
    options.AccessDeniedPath = "/Compte/Connexion";
    options.ReturnUrlParameter = "retour";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    // L'API répond 401 au lieu de rediriger vers la page de connexion.
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("HxhGuide");

builder.Services.AddSingleton<CharacterImages>();

builder.Services.AddRazorPages(options =>
{
    // Le guide est public. Seul le classeur (suivi de lecture) demande un compte ;
    // les avis et favoris sont vérifiés dans leurs actions.
    options.Conventions.AuthorizePage("/Classeur");
    options.Conventions.AddFolderApplicationModelConvention("/Compte",
        model => model.EndpointMetadata.Add(new EnableRateLimitingAttribute("compte")));
});

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Laisse les accents tels quels dans le HTML (« é » au lieu de « &#xE9; ») : pages plus légères et lisibles.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Ne pas annoncer le serveur web utilisé (en-tête « Server: Kestrel »).
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Anti force brute : 10 envois de formulaire de compte (connexion, inscription) par minute et par adresse IP.
// S'ajoute au blocage du compte après 5 mots de passe erronés.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("compte", context =>
        HttpMethods.IsPost(context.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "inconnue",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("lecture"));
});

var app = builder.Build();

// Applique les migrations (création/évolution du schéma) au démarrage, puis les données de démo si demandé.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("Seed:Demo"))
    {
        await SeedData.SeedDemoAsync(scope.ServiceProvider);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// En-têtes de sécurité sur toutes les réponses.
// La CSP n'autorise que les scripts, polices et styles servis par le site lui-même :
// un script injecté (XSS) ne pourrait ni s'exécuter ni envoyer de données ailleurs.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; " +
        "font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    headers["Cross-Origin-Opener-Policy"] = "same-origin";
    await next();
});

// Pages d'erreur HTML (404...) pour le site, mais pas pour l'API qui garde des réponses JSON.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    site => site.UseStatusCodePagesWithReExecute("/Error", "?code={0}"));

app.UseRequestLocalization(options => options
    .SetDefaultCulture("fr-FR")
    .AddSupportedCultures("fr-FR")
    .AddSupportedUICultures("fr-FR"));

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGuideApi();
app.MapHealth();

app.Run();
