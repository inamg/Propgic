namespace Propgic.Application.DTOs;

public class PropertyDataDto
{
    // 1. Property type (6%)
    public string PropertyType { get; set; } = string.Empty; // House, Unit, Townhouse, etc.

    // 2. Land ownership (5%)
    public string LandOwnership { get; set; } = string.Empty; // Freehold, Leasehold, Strata

    // 3. Title clarity (8%)
    public bool HasClearTitle { get; set; }
    public bool HasEncumbrances { get; set; }

    // 4. Zoning (4%)
    public string Zoning { get; set; } = string.Empty; // Residential, Commercial, Mixed

    // 5. Location category (5%)
    public string LocationCategory { get; set; } = string.Empty; // Metro, Regional, Rural

    // 6. Proximity to CBD (3%)
    public int DistanceToCbdKm { get; set; }

    // 7. School zone quality (4%)
    public string SchoolZoneQuality { get; set; } = string.Empty; // Top-tier, Good, Average

    // 8. Public transport (3%)
    public int DistanceToPublicTransportMeters { get; set; }

    // 9. Rental yield (7%)
    public decimal RentalYieldPercentage { get; set; }

    // 10. Capital growth trend (6%)
    public decimal CapitalGrowthPercentage { get; set; } // 5-year average

    // 11. Vacancy rate (4%)
    public decimal VacancyRatePercentage { get; set; }

    // 12. Local area demand (5%)
    public string LocalDemand { get; set; } = string.Empty; // High, Medium, Low

    // 13. Structural soundness (6%)
    public bool HasStructuralIssues { get; set; }
    public int PropertyAgeYears { get; set; }

    // 14. Major defects (5%)
    public bool HasMajorDefects { get; set; }

    // 15. Maintenance required (3%)
    public string MaintenanceLevel { get; set; } = string.Empty; // Minimal, Moderate, Extensive

    // 16. Compliance (4%)
    public bool MeetsCurrentBuildingCodes { get; set; }
    public bool HasRequiredCertificates { get; set; }

    // 17. Tenant quality (3%)
    public bool HasLongTermTenants { get; set; }
    public bool HasReliablePaymentHistory { get; set; }

    // 18. Lease status (3%)
    public int LeaseRemainingMonths { get; set; }

    // 19. Rental consistency (2%)
    public bool HasConsistentRentalHistory { get; set; }

    // 20. Cash flow coverage (5%)
    public decimal CashFlowCoverageRatio { get; set; } // Rental income / Loan repayment

    // 21. Loan serviceability (4%)
    public bool MeetsServiceabilityRequirements { get; set; }

    // 22. Equity buffer (3%)
    public decimal LoanToValueRatio { get; set; }

    // 23. Insurance costs (2%)
    public decimal AnnualInsuranceCost { get; set; }

    // 24. Cross-collateral suitability (4%)
    public bool SuitableForCrossCollateral { get; set; }

    // 25. Borrowing capacity boost (3%)
    public decimal EquityAvailable { get; set; }

    // 26. Refinance eligibility (2%)
    public bool EligibleForRefinance { get; set; }

    // 27. Sale history (2%)
    public bool HasStableSaleHistory { get; set; }
    public int YearsSinceLastSale { get; set; }

    // 28. Market activity (2%)
    public int DaysOnMarket { get; set; }

    // 29. Comparable sales (2%)
    public bool HasStrongComparables { get; set; }

    // 30. Uniqueness/scarcity (1%)
    public bool IsUniqueProperty { get; set; }

    // 31. Lender acceptance (3%)
    public bool AcceptedByMajorLenders { get; set; }

    // 32. Risk rating (2%)
    public string RiskRating { get; set; } = string.Empty; // Low, Medium, High

    // 33. Future development risk (1%)
    public bool HasDevelopmentRisk { get; set; }

    // 34. Portfolio diversity fit (1%)
    public bool FitsPortfolioDiversity { get; set; }

    // 35. Long-term hold viability (1%)
    public bool ViableForLongTermHold { get; set; }
}
