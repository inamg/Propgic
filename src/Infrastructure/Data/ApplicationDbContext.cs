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
    public DbSet<PropertyData> PropertyDataRecords { get; set; }

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

        // Configure PropertyData entity
        modelBuilder.Entity<PropertyData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PropertyAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PropertyUrl).HasMaxLength(1000);
            entity.Property(e => e.PropertyType).HasMaxLength(50);
            entity.Property(e => e.LandOwnership).HasMaxLength(50);
            entity.Property(e => e.Zoning).HasMaxLength(50);
            entity.Property(e => e.LocationCategory).HasMaxLength(50);
            entity.Property(e => e.SchoolZoneQuality).HasMaxLength(50);
            entity.Property(e => e.LocalDemand).HasMaxLength(50);
            entity.Property(e => e.MaintenanceLevel).HasMaxLength(50);
            entity.Property(e => e.RiskRating).HasMaxLength(50);
            entity.Property(e => e.DataSource).HasMaxLength(100);
            entity.Property(e => e.RentalYieldPercentage).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CapitalGrowthPercentage).HasColumnType("decimal(18,2)");
            entity.Property(e => e.VacancyRatePercentage).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CashFlowCoverageRatio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LoanToValueRatio).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AnnualInsuranceCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.EquityAvailable).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.PropertyAddress);
        });
    }
}
