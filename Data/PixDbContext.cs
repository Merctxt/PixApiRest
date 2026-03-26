using Microsoft.EntityFrameworkCore;
using PixApiRest.Entities;

namespace PixApiRest.Data;

public class PixDbContext : DbContext
{
    public PixDbContext(DbContextOptions<PixDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Status)
                  .HasConversion<string>();

            // SQLite does not have a native decimal type; store as TEXT to preserve precision
            entity.Property(e => e.Amount)
                  .HasColumnType("TEXT");
        });
    }
}
