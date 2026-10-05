using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite ne garde pas le fuseau horaire : on indique que les dates relues sont en UTC.
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
