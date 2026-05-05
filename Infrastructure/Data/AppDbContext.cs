using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
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

        // Configure the Vulnerability Table
        modelBuilder.Entity<Vulnerability>(entity =>
        {
            // Map Descriptions to a JSON column
            entity.OwnsMany(v => v.Descriptions, d =>
            {
                d.ToJson();
            });

            // Map References to a JSON column
            entity.OwnsMany(v => v.References, r =>
            {
                r.ToJson();
            });

            // Index the base score so sorting by severity is fast
            entity.HasIndex(v => v.CvssV31BaseScore);
        });

        // Configure the CpeMatch Table
        modelBuilder.Entity<CpeMatch>(entity =>
        {
            // Set up the Foreign Key constraint
            entity.HasOne(c => c.Vulnerability)
                  .WithMany(v => v.CpeMatches)
                  .HasForeignKey(c => c.VulnerabilityId)
                  .OnDelete(DeleteBehavior.Cascade);

            // THIS IS THE MOST IMPORTANT LINE IN THE ENTIRE DATABASE
            // Without this index, the application will crash when trying to search CPEs
            entity.HasIndex(c => c.Criteria); 
            
            // A composite index for searching by CPE + Vulnerable flag
            entity.HasIndex(c => new { c.Criteria, c.Vulnerable });
        });
    }
}