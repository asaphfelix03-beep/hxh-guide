using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Persistence;
using TaskFlow.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Dossier de données : base SQLite + clés de chiffrement (cookies, anti-CSRF).
// Dans le conteneur c'est /app/data, monté sur un volume Docker pour survivre aux redémarrages.
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataDir);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(dataDir, "tasks.db")}"));

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDir, "keys")))
    .SetApplicationName("TaskFlow");

builder.Services.AddRazorPages();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Crée la base au premier démarrage et ajoute quelques tâches d'exemple.
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Pas de UseHttpsRedirection : sur EC2 l'application est servie en HTTP sur le port 80.
app.UseRequestLocalization(options => options
    .SetDefaultCulture("fr-FR")
    .AddSupportedCultures("fr-FR")
    .AddSupportedUICultures("fr-FR"));

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapTaskApi();
app.MapHealth();

app.Run();
