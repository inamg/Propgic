using Propgic.Domain.Common;

namespace Propgic.Domain.Entities;

public class PropertyData : BaseEntity
{
    public string PropertyAddress { get; set; } = string.Empty;
    public string? PropertyUrl { get; set; }

    // 1. Property type (6%)
    public string? PropertyType { get; set; }

    // 2. Land ownership (5%)
    public string? LandOwnership { get; set; }

    // 3. Title clarity (8%)
    public bool? HasClearTitle { get; set; }
    public bool? HasEncumbrances { get; set; }

    // 4. Zoning (4%)
    public string? Zoning { get; set; }

    // 5. Location category (5%)
    public string? LocationCategory { get; set; }

    // 6. Proximity to CBD (3%)
    public int? DistanceToCbdKm { get; set; }

    // 7. School zone quality (4%)
    public string? SchoolZoneQuality { get; set; }

    // 8. Public transport (3%)
    public int? DistanceToPublicTransportMeters { get; set; }

    // 9. Rental yield (7%)
    public decimal? RentalYieldPercentage { get; set; }

    // 10. Capital growth trend (6%)
    public decimal? CapitalGrowthPercentage { get; set; }

    // 11. Vacancy rate (4%)
    public decimal? VacancyRatePercentage { get; set; }

    // 12. Local area demand (5%)
    public string? LocalDemand { get; set; }

    // 13. Structural soundness (6%)
    public bool? HasStructuralIssues { get; set; }
    public int? PropertyAgeYears { get; set; }

    // 14. Major defects (5%)
    public bool? HasMajorDefects { get; set; }

    // 15. Maintenance required (3%)
    public string? MaintenanceLevel { get; set; }

    // 16. Compliance (4%)
    public bool? MeetsCurrentBuildingCodes { get; set; }
    public bool? HasRequiredCertificates { get; set; }

    // 17. Tenant quality (3%)
    public bool? HasLongTermTenants { get; set; }
    public bool? HasReliablePaymentHistory { get; set; }

    // 18. Lease status (3%)
    public int? LeaseRemainingMonths { get; set; }

    // 19. Rental consistency (2%)
    public bool? HasConsistentRentalHistory { get; set; }

    // 20. Cash flow coverage (5%)
    public decimal? CashFlowCoverageRatio { get; set; }

    // 21. Loan serviceability (4%)
    public bool? MeetsServiceabilityRequirements { get; set; }

    // 22. Equity buffer (3%)
    public decimal? LoanToValueRatio { get; set; }

    // 23. Insurance costs (2%)
    public decimal? AnnualInsuranceCost { get; set; }

    // 24. Cross-collateral suitability (4%)
    public bool? SuitableForCrossCollateral { get; set; }

    // 25. Borrowing capacity boost (3%)
    public decimal? EquityAvailable { get; set; }

    // 26. Refinance eligibility (2%)
    public bool? EligibleForRefinance { get; set; }

    // 27. Sale history (2%)
    public bool? HasStableSaleHistory { get; set; }
    public int? YearsSinceLastSale { get; set; }

    // 28. Market activity (2%)
    public int? DaysOnMarket { get; set; }

    // 29. Comparable sales (2%)
    public bool? HasStrongComparables { get; set; }

    // 30. Uniqueness/scarcity (1%)
    public bool? IsUniqueProperty { get; set; }

    // 31. Lender acceptance (3%)
    public bool? AcceptedByMajorLenders { get; set; }

    // 32. Risk rating (2%)
    public string? RiskRating { get; set; }

    // 33. Future development risk (1%)
    public bool? HasDevelopmentRisk { get; set; }

    // 34. Portfolio diversity fit (1%)
    public bool? FitsPortfolioDiversity { get; set; }

    // 35. Long-term hold viability (1%)
    public bool? ViableForLongTermHold { get; set; }

    // Data source tracking
    public string? DataSource { get; set; } // e.g., "OpenAI", "Domain.com.au", "Aggregated"
}
