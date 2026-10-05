using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options), IDataProtectionKeyContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // Clés de chiffrement des cookies stockées en base : les sessions survivent au remplacement du conteneur.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(user =>
        {
            user.Property(u => u.DisplayName).HasMaxLength(60);
            user.Ignore(u => u.Initials);
        });

        builder.Entity<ProjectMember>(member =>
        {
            member.HasKey(m => new { m.ProjectId, m.UserId });
            member.HasOne(m => m.Project).WithMany(p => p.Members).OnDelete(DeleteBehavior.Cascade);
            member.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TaskItem>(task =>
        {
            task.ToTable("Tasks");
            task.HasOne(t => t.Project).WithMany(p => p.Tasks).OnDelete(DeleteBehavior.Cascade);
            task.HasOne(t => t.Assignee).WithMany().HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            task.HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.SetNull);
            task.HasIndex(t => new { t.ProjectId, t.State, t.Position });
            task.HasIndex(t => t.AssigneeId);
            task.Ignore(t => t.TagList);
        });
    }
}
