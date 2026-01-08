using Propgic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Propgic.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PropertyAnalysis> PropertyAnalyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PropertyAnalysis entity
        modelBuilder.Entity<PropertyAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PropertyAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AnalyserType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AnalysisScore).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Remarks).HasMaxLength(2000);
            entity.HasIndex(e => e.AnalyserType);
            entity.HasIndex(e => e.Status);
        });
    }
}
