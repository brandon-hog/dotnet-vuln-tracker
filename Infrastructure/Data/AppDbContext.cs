using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Vulnerability> Vulnerabilities => Set<Vulnerability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Hostname).IsRequired().HasMaxLength(100);
            builder.Property(a => a.IpAddress).IsRequired().HasMaxLength(45); // IPv6 max length

            // Tells EF Core to populate the private _vulnerabilities backing field
            // preserving Domain encapsulation principles.
            builder.Metadata.FindNavigation(nameof(Asset.Vulnerabilities))?
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Vulnerability>(builder =>
        {
            builder.HasKey(v => v.CveId);
            builder.Property(v => v.Description).IsRequired();
            builder.Property(v => v.CvssScore).HasPrecision(4, 2); // Security precision
        });
    }
}