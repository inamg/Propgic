using Propgic.Application.Interfaces;
using Propgic.Application.DTOs;
using Propgic.Application.Services.PropertyDataFetchers;
using Propgic.Domain.Entities;

namespace Propgic.Application.Services.Analysers;

public class PropertyAnchorUrlAnalyser : IPropertyAnalyser
{
    private readonly PropertyDataAggregator _dataAggregator;

    public string AnalyserType => "PropertyAnchorUrl";

    public PropertyAnchorUrlAnalyser(PropertyDataAggregator dataAggregator)
    {
        _dataAggregator = dataAggregator;
    }

    public async Task<PropertyAnalysis> AnalyseAsync(PropertyAnalysis propertyAnalysis)
    {
        // Update status to InProgress
        propertyAnalysis.Status = "InProgress";

        try
        {
            // Fetch property data directly from URL
            var propertyData = await FetchPropertyDataFromUrlAsync(propertyAnalysis.PropertyAddress);

            // Perform analysis using the same scoring logic as PropertyAnchorAnalyser
            var score = CalculateAnchorScore(propertyData);
            var result = GenerateAnalysisResult(score);

            // Update the property analysis with results
            propertyAnalysis.AnalysisScore = score;
            propertyAnalysis.AnalysisResult = result;
            propertyAnalysis.Status = "Completed";
            propertyAnalysis.CompletedAt = DateTime.UtcNow;
            propertyAnalysis.Remarks = "Analysis completed successfully using PropertyAnchorUrl analyzer with 35 weighted attributes from direct URL";

            return propertyAnalysis;
        }
        catch (Exception ex)
        {
            propertyAnalysis.Status = "Failed";
            propertyAnalysis.Remarks = $"Analysis failed: {ex.Message}";
            return propertyAnalysis;
        }
    }

    private async Task<PropertyDataDto> FetchPropertyDataFromUrlAsync(string propertyUrl)
    {
        // Fetch data directly from the provided URL
        return await _dataAggregator.FetchFromUrlAsync(propertyUrl);
    }

    private decimal CalculateAnchorScore(PropertyDataDto propertyData)
    {
        decimal totalScore = 0m;

        // 1. Property type (6%)
        totalScore += EvaluatePropertyType(propertyData.PropertyType) * 0.06m;

        // 2. Land ownership (5%)
        totalScore += EvaluateLandOwnership(propertyData.LandOwnership) * 0.05m;

        // 3. Title clarity (8%)
        totalScore += EvaluateTitleClarity(propertyData.HasClearTitle, propertyData.HasEncumbrances) * 0.08m;

        // 4. Zoning (4%)
        totalScore += EvaluateZoning(propertyData.Zoning) * 0.04m;

        // 5. Location category (5%)
        totalScore += EvaluateLocationCategory(propertyData.LocationCategory) * 0.05m;

        // 6. Proximity to CBD (3%)
        totalScore += EvaluateProximityToCbd(propertyData.DistanceToCbdKm) * 0.03m;

        // 7. School zone quality (4%)
        totalScore += EvaluateSchoolZone(propertyData.SchoolZoneQuality) * 0.04m;

        // 8. Public transport proximity (3%)
        totalScore += EvaluatePublicTransportProximity(propertyData.DistanceToPublicTransportMeters) * 0.03m;

        // 9. Rental yield (5%)
        totalScore += EvaluateRentalYield(propertyData.RentalYieldPercentage) * 0.05m;

        // 10. Capital growth potential (6%)
        totalScore += EvaluateCapitalGrowth(propertyData.CapitalGrowthPercentage) * 0.06m;

        // 11. Vacancy rate (4%)
        totalScore += EvaluateVacancyRate(propertyData.VacancyRatePercentage) * 0.04m;

        // 12. Local demand (4%)
        totalScore += EvaluateLocalDemand(propertyData.LocalDemand) * 0.04m;

        // 13. Structural condition (5%)
        totalScore += EvaluateStructuralCondition(propertyData.HasStructuralIssues) * 0.05m;

        // 14. Property age (3%)
        totalScore += EvaluatePropertyAge(propertyData.PropertyAgeYears) * 0.03m;

        // 15. Major defects (5%)
        totalScore += EvaluateMajorDefects(propertyData.HasMajorDefects) * 0.05m;

        // 16. Maintenance requirements (3%)
        totalScore += EvaluateMaintenanceLevel(propertyData.MaintenanceLevel) * 0.03m;

        // 17. Building code compliance (4%)
        totalScore += EvaluateBuildingCodes(propertyData.MeetsCurrentBuildingCodes) * 0.04m;

        // 18. Required certificates (3%)
        totalScore += EvaluateCertificates(propertyData.HasRequiredCertificates) * 0.03m;

        // 19. Tenant quality (3%)
        totalScore += EvaluateTenantQuality(propertyData.HasLongTermTenants) * 0.03m;

        // 20. Payment history (3%)
        totalScore += EvaluatePaymentHistory(propertyData.HasReliablePaymentHistory) * 0.03m;

        // 21. Lease terms (2%)
        totalScore += EvaluateLeaseTerms(propertyData.LeaseRemainingMonths) * 0.02m;

        // 22. Rental history consistency (2%)
        totalScore += EvaluateRentalHistory(propertyData.HasConsistentRentalHistory) * 0.02m;

        // 23. Cash flow coverage (4%)
        totalScore += EvaluateCashFlowCoverage(propertyData.CashFlowCoverageRatio) * 0.04m;

        // 24. Serviceability (3%)
        totalScore += EvaluateServiceability(propertyData.MeetsServiceabilityRequirements) * 0.03m;

        // 25. Loan-to-value ratio (3%)
        totalScore += EvaluateLVR(propertyData.LoanToValueRatio) * 0.03m;

        // 26. Insurance costs (2%)
        totalScore += EvaluateInsuranceCosts(propertyData.AnnualInsuranceCost) * 0.02m;

        // 27. Cross-collateral suitability (2%)
        totalScore += EvaluateCrossCollateral(propertyData.SuitableForCrossCollateral) * 0.02m;

        // 28. Available equity (3%)
        totalScore += EvaluateEquity(propertyData.EquityAvailable) * 0.03m;

        // 29. Refinance eligibility (2%)
        totalScore += EvaluateRefinance(propertyData.EligibleForRefinance) * 0.02m;

        // 30. Sale history stability (2%)
        totalScore += EvaluateSaleHistory(propertyData.HasStableSaleHistory) * 0.02m;

        // 31. Time since last sale (2%)
        totalScore += EvaluateTimeSinceLastSale(propertyData.YearsSinceLastSale) * 0.02m;

        // 32. Market exposure (2%)
        totalScore += EvaluateDaysOnMarket(propertyData.DaysOnMarket) * 0.02m;

        // 33. Comparable properties (2%)
        totalScore += EvaluateComparables(propertyData.HasStrongComparables) * 0.02m;

        // 34. Property uniqueness (1%)
        totalScore += EvaluateUniqueness(propertyData.IsUniqueProperty) * 0.01m;

        // 35. Lender acceptance (2%)
        totalScore += EvaluateLenderAcceptance(propertyData.AcceptedByMajorLenders) * 0.02m;

        return Math.Round(totalScore, 2);
    }

    // Evaluation methods (same as PropertyAnchorAnalyser)
    private decimal EvaluatePropertyType(string propertyType) =>
        propertyType switch
        {
            "House" => 100m,
            "Townhouse" => 85m,
            "Unit" => 75m,
            "Duplex" => 80m,
            _ => 60m
        };

    private decimal EvaluateLandOwnership(string ownership) =>
        ownership switch
        {
            "Freehold" => 100m,
            "Strata" => 70m,
            "Leasehold" => 50m,
            _ => 40m
        };

    private decimal EvaluateTitleClarity(bool hasClearTitle, bool hasEncumbrances)
    {
        if (hasClearTitle && !hasEncumbrances) return 100m;
        if (hasClearTitle && hasEncumbrances) return 60m;
        if (!hasClearTitle && !hasEncumbrances) return 50m;
        return 30m;
    }

    private decimal EvaluateZoning(string zoning) =>
        zoning switch
        {
            "Residential" => 100m,
            "Mixed Use" => 75m,
            "Commercial" => 60m,
            "Industrial" => 50m,
            _ => 40m
        };

    private decimal EvaluateLocationCategory(string category) =>
        category switch
        {
            "Metro" => 100m,
            "Regional" => 70m,
            "Rural" => 50m,
            _ => 60m
        };

    private decimal EvaluateProximityToCbd(int distanceKm)
    {
        if (distanceKm <= 5) return 100m;
        if (distanceKm <= 10) return 90m;
        if (distanceKm <= 20) return 75m;
        if (distanceKm <= 30) return 60m;
        return 40m;
    }

    private decimal EvaluateSchoolZone(string quality) =>
        quality switch
        {
            "Top-tier" => 100m,
            "Good" => 80m,
            "Average" => 60m,
            _ => 40m
        };

    private decimal EvaluatePublicTransportProximity(int distanceMeters)
    {
        if (distanceMeters <= 400) return 100m;
        if (distanceMeters <= 800) return 85m;
        if (distanceMeters <= 1500) return 70m;
        return 50m;
    }

    private decimal EvaluateRentalYield(decimal yieldPercentage)
    {
        if (yieldPercentage >= 6.0m) return 100m;
        if (yieldPercentage >= 5.0m) return 85m;
        if (yieldPercentage >= 4.0m) return 70m;
        if (yieldPercentage >= 3.0m) return 55m;
        return 40m;
    }

    private decimal EvaluateCapitalGrowth(decimal growthPercentage)
    {
        if (growthPercentage >= 8.0m) return 100m;
        if (growthPercentage >= 6.0m) return 85m;
        if (growthPercentage >= 4.0m) return 70m;
        if (growthPercentage >= 2.0m) return 55m;
        return 40m;
    }

    private decimal EvaluateVacancyRate(decimal vacancyRate)
    {
        if (vacancyRate <= 1.0m) return 100m;
        if (vacancyRate <= 2.0m) return 85m;
        if (vacancyRate <= 3.0m) return 70m;
        if (vacancyRate <= 5.0m) return 50m;
        return 30m;
    }

    private decimal EvaluateLocalDemand(string demand) =>
        demand switch
        {
            "High" => 100m,
            "Medium" => 70m,
            "Low" => 40m,
            _ => 50m
        };

    private decimal EvaluateStructuralCondition(bool hasIssues) => hasIssues ? 30m : 100m;

    private decimal EvaluatePropertyAge(int ageYears)
    {
        if (ageYears <= 5) return 100m;
        if (ageYears <= 10) return 90m;
        if (ageYears <= 20) return 75m;
        if (ageYears <= 30) return 60m;
        return 40m;
    }

    private decimal EvaluateMajorDefects(bool hasDefects) => hasDefects ? 20m : 100m;

    private decimal EvaluateMaintenanceLevel(string level) =>
        level switch
        {
            "Minimal" => 100m,
            "Moderate" => 70m,
            "Extensive" => 40m,
            _ => 60m
        };

    private decimal EvaluateBuildingCodes(bool meetsCode) => meetsCode ? 100m : 40m;

    private decimal EvaluateCertificates(bool hasCertificates) => hasCertificates ? 100m : 50m;

    private decimal EvaluateTenantQuality(bool hasLongTermTenants) => hasLongTermTenants ? 100m : 60m;

    private decimal EvaluatePaymentHistory(bool hasReliableHistory) => hasReliableHistory ? 100m : 50m;

    private decimal EvaluateLeaseTerms(int monthsRemaining)
    {
        if (monthsRemaining >= 12) return 100m;
        if (monthsRemaining >= 6) return 70m;
        if (monthsRemaining >= 3) return 50m;
        return 30m;
    }

    private decimal EvaluateRentalHistory(bool hasConsistentHistory) => hasConsistentHistory ? 100m : 60m;

    private decimal EvaluateCashFlowCoverage(decimal ratio)
    {
        if (ratio >= 1.5m) return 100m;
        if (ratio >= 1.3m) return 85m;
        if (ratio >= 1.2m) return 70m;
        if (ratio >= 1.0m) return 50m;
        return 30m;
    }

    private decimal EvaluateServiceability(bool meetsRequirements) => meetsRequirements ? 100m : 40m;

    private decimal EvaluateLVR(decimal lvr)
    {
        if (lvr <= 60m) return 100m;
        if (lvr <= 70m) return 85m;
        if (lvr <= 80m) return 70m;
        if (lvr <= 90m) return 50m;
        return 30m;
    }

    private decimal EvaluateInsuranceCosts(decimal annualCost)
    {
        if (annualCost <= 1000m) return 100m;
        if (annualCost <= 1500m) return 85m;
        if (annualCost <= 2000m) return 70m;
        if (annualCost <= 2500m) return 55m;
        return 40m;
    }

    private decimal EvaluateCrossCollateral(bool isSuitable) => isSuitable ? 100m : 50m;

    private decimal EvaluateEquity(decimal equity)
    {
        if (equity >= 200000m) return 100m;
        if (equity >= 150000m) return 85m;
        if (equity >= 100000m) return 70m;
        if (equity >= 50000m) return 55m;
        return 40m;
    }

    private decimal EvaluateRefinance(bool isEligible) => isEligible ? 100m : 50m;

    private decimal EvaluateSaleHistory(bool hasStableHistory) => hasStableHistory ? 100m : 60m;

    private decimal EvaluateTimeSinceLastSale(int years)
    {
        if (years >= 5) return 100m;
        if (years >= 3) return 85m;
        if (years >= 2) return 70m;
        if (years >= 1) return 55m;
        return 40m;
    }

    private decimal EvaluateDaysOnMarket(int days)
    {
        if (days <= 14) return 100m;
        if (days <= 30) return 85m;
        if (days <= 60) return 70m;
        if (days <= 90) return 55m;
        return 40m;
    }

    private decimal EvaluateComparables(bool hasStrongComparables) => hasStrongComparables ? 100m : 60m;

    private decimal EvaluateUniqueness(bool isUnique) => isUnique ? 60m : 100m;

    private decimal EvaluateLenderAcceptance(bool acceptedByMajorLenders) => acceptedByMajorLenders ? 100m : 40m;

    private string GenerateAnalysisResult(decimal score)
    {
        if (score >= 85) return "Excellent Anchor Property - Highly recommended for portfolio";
        if (score >= 75) return "Very Good Anchor Property - Recommended with minor considerations";
        if (score >= 65) return "Good Anchor Property - Suitable with some conditions";
        if (score >= 50) return "Fair Anchor Property - Requires careful evaluation";
        return "Below Standard - Not recommended as anchor property";
    }
}
