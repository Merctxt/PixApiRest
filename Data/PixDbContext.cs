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
            entity.HasIndex(e => e.Txid).IsUnique();
            
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });
    }
}
