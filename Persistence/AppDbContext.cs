using HxhGuide.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HxhGuide.Persistence;

/// <summary>
/// Seules les données des lecteurs sont en base (tomes lus, avis, favoris).
/// Le contenu du manga (arcs, personnages) est dans <see cref="Catalog"/>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Reader>(options), IDataProtectionKeyContext
{
    public DbSet<ReadVolume> ReadVolumes => Set<ReadVolume>();
    public DbSet<ArcReview> ArcReviews => Set<ArcReview>();
    public DbSet<FavoriteCharacter> FavoriteCharacters => Set<FavoriteCharacter>();

    // Clés de chiffrement des cookies stockées en base : les sessions survivent aux mises à jour.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Reader>().Property(r => r.DisplayName).HasMaxLength(60);

        builder.Entity<ReadVolume>(volume =>
        {
            volume.HasKey(v => new { v.UserId, v.Number });
            volume.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ArcReview>(review =>
        {
            review.HasIndex(r => new { r.UserId, r.ArcSlug }).IsUnique();
            review.HasIndex(r => r.ArcSlug);
            review.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FavoriteCharacter>(favorite =>
        {
            favorite.HasKey(f => new { f.UserId, f.CharacterSlug });
            favorite.HasIndex(f => f.CharacterSlug);
            favorite.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
