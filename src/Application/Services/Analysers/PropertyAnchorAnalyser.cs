using Propgic.Application.Interfaces;
using Propgic.Application.DTOs;
using Propgic.Application.Services.PropertyDataFetchers;
using Propgic.Domain.Entities;

namespace Propgic.Application.Services.Analysers;

public class PropertyAnchorAnalyser : IPropertyAnalyser
{
    private readonly PropertyDataAggregator _dataAggregator;

    public string AnalyserType => "PropertyAnchor";

    public PropertyAnchorAnalyser(PropertyDataAggregator dataAggregator)
    {
        _dataAggregator = dataAggregator;
    }

    public async Task<PropertyAnalysis> AnalyseAsync(PropertyAnalysis propertyAnalysis)
    {
        // Update status to InProgress
        propertyAnalysis.Status = "InProgress";

        try
        {
            // Fetch property data based on SourceType
            PropertyDataDto propertyData;
            if (propertyAnalysis.SourceType == "Url")
            {
                propertyData = await _dataAggregator.FetchFromUrlAsync(propertyAnalysis.PropertyAddress);
            }
            else
            {
                propertyData = await _dataAggregator.FetchAndAggregateAsync(propertyAnalysis.PropertyAddress);
            }

            // Perform analysis
            var score = CalculateAnchorScore(propertyData);
            var result = GenerateAnalysisResult(score);

            // Update the property analysis with results
            propertyAnalysis.AnalysisScore = score;
            propertyAnalysis.AnalysisResult = result;
            propertyAnalysis.Status = "Completed";
            propertyAnalysis.CompletedAt = DateTime.UtcNow;

            if (propertyAnalysis.SourceType == "Url")
            {
                propertyAnalysis.Remarks = "Analysis completed successfully using PropertyAnchor analyzer with 35 weighted attributes from direct URL";
            }
            else
            {
                propertyAnalysis.Remarks = "Analysis completed successfully using PropertyAnchor analyzer with 35 weighted attributes from web sources";
            }

            return propertyAnalysis;
        }
        catch (Exception ex)
        {
            propertyAnalysis.Status = "Failed";
            propertyAnalysis.Remarks = $"Analysis failed: {ex.Message}";
            return propertyAnalysis;
        }
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

        // 8. Public transport (3%)
        totalScore += EvaluatePublicTransport(propertyData.DistanceToPublicTransportMeters) * 0.03m;

        // 9. Rental yield (7%)
        totalScore += EvaluateRentalYield(propertyData.RentalYieldPercentage) * 0.07m;

        // 10. Capital growth trend (6%)
        totalScore += EvaluateCapitalGrowth(propertyData.CapitalGrowthPercentage) * 0.06m;

        // 11. Vacancy rate (4%)
        totalScore += EvaluateVacancyRate(propertyData.VacancyRatePercentage) * 0.04m;

        // 12. Local area demand (5%)
        totalScore += EvaluateLocalDemand(propertyData.LocalDemand) * 0.05m;

        // 13. Structural soundness (6%)
        totalScore += EvaluateStructuralSoundness(propertyData.HasStructuralIssues, propertyData.PropertyAgeYears) * 0.06m;

        // 14. Major defects (5%)
        totalScore += EvaluateMajorDefects(propertyData.HasMajorDefects) * 0.05m;

        // 15. Maintenance required (3%)
        totalScore += EvaluateMaintenance(propertyData.MaintenanceLevel) * 0.03m;

        // 16. Compliance (4%)
        totalScore += EvaluateCompliance(propertyData.MeetsCurrentBuildingCodes, propertyData.HasRequiredCertificates) * 0.04m;

        // 17. Tenant quality (3%)
        totalScore += EvaluateTenantQuality(propertyData.HasLongTermTenants, propertyData.HasReliablePaymentHistory) * 0.03m;

        // 18. Lease status (3%)
        totalScore += EvaluateLeaseStatus(propertyData.LeaseRemainingMonths) * 0.03m;

        // 19. Rental consistency (2%)
        totalScore += EvaluateRentalConsistency(propertyData.HasConsistentRentalHistory) * 0.02m;

        // 20. Cash flow coverage (5%)
        totalScore += EvaluateCashFlowCoverage(propertyData.CashFlowCoverageRatio) * 0.05m;

        // 21. Loan serviceability (4%)
        totalScore += EvaluateLoanServiceability(propertyData.MeetsServiceabilityRequirements) * 0.04m;

        // 22. Equity buffer (3%)
        totalScore += EvaluateEquityBuffer(propertyData.LoanToValueRatio) * 0.03m;

        // 23. Insurance costs (2%)
        totalScore += EvaluateInsuranceCosts(propertyData.AnnualInsuranceCost) * 0.02m;

        // 24. Cross-collateral suitability (4%)
        totalScore += EvaluateCrossCollateralSuitability(propertyData.SuitableForCrossCollateral) * 0.04m;

        // 25. Borrowing capacity boost (3%)
        totalScore += EvaluateBorrowingCapacity(propertyData.EquityAvailable) * 0.03m;

        // 26. Refinance eligibility (2%)
        totalScore += EvaluateRefinanceEligibility(propertyData.EligibleForRefinance) * 0.02m;

        // 27. Sale history (2%)
        totalScore += EvaluateSaleHistory(propertyData.HasStableSaleHistory, propertyData.YearsSinceLastSale) * 0.02m;

        // 28. Market activity (2%)
        totalScore += EvaluateMarketActivity(propertyData.DaysOnMarket) * 0.02m;

        // 29. Comparable sales (2%)
        totalScore += EvaluateComparableSales(propertyData.HasStrongComparables) * 0.02m;

        // 30. Uniqueness/scarcity (1%)
        totalScore += EvaluateUniqueness(propertyData.IsUniqueProperty) * 0.01m;

        // 31. Lender acceptance (3%)
        totalScore += EvaluateLenderAcceptance(propertyData.AcceptedByMajorLenders) * 0.03m;

        // 32. Risk rating (2%)
        totalScore += EvaluateRiskRating(propertyData.RiskRating) * 0.02m;

        // 33. Future development risk (1%)
        totalScore += EvaluateDevelopmentRisk(propertyData.HasDevelopmentRisk) * 0.01m;

        // 34. Portfolio diversity fit (1%)
        totalScore += EvaluatePortfolioDiversity(propertyData.FitsPortfolioDiversity) * 0.01m;

        // 35. Long-term hold viability (1%)
        totalScore += EvaluateLongTermViability(propertyData.ViableForLongTermHold) * 0.01m;

        return Math.Round(totalScore, 2);
    }

    // Evaluation methods for each attribute (return score 0-100)
    private decimal EvaluatePropertyType(string propertyType)
    {
        return propertyType.ToLower() switch
        {
            "house" => 100m,
            "townhouse" => 90m,
            "unit" or "apartment" => 80m,
            "duplex" => 85m,
            _ => 50m
        };
    }

    private decimal EvaluateLandOwnership(string landOwnership)
    {
        return landOwnership.ToLower() switch
        {
            "freehold" => 100m,
            "strata" => 75m,
            "leasehold" => 50m,
            _ => 40m
        };
    }

    private decimal EvaluateTitleClarity(bool hasClearTitle, bool hasEncumbrances)
    {
        if (hasClearTitle && !hasEncumbrances) return 100m;
        if (hasClearTitle && hasEncumbrances) return 70m;
        if (!hasClearTitle && !hasEncumbrances) return 60m;
        return 30m;
    }

    private decimal EvaluateZoning(string zoning)
    {
        return zoning.ToLower() switch
        {
            "residential" => 100m,
            "mixed" => 85m,
            "commercial" => 70m,
            "industrial" => 50m,
            _ => 40m
        };
    }

    private decimal EvaluateLocationCategory(string locationCategory)
    {
        return locationCategory.ToLower() switch
        {
            "metro" or "metropolitan" => 100m,
            "regional" => 75m,
            "rural" => 50m,
            _ => 40m
        };
    }

    private decimal EvaluateProximityToCbd(int distanceKm)
    {
        if (distanceKm <= 10) return 100m;
        if (distanceKm <= 20) return 85m;
        if (distanceKm <= 30) return 70m;
        if (distanceKm <= 50) return 55m;
        return 40m;
    }

    private decimal EvaluateSchoolZone(string quality)
    {
        return quality.ToLower() switch
        {
            "top-tier" or "excellent" => 100m,
            "good" => 85m,
            "average" => 70m,
            "below average" => 50m,
            _ => 40m
        };
    }

    private decimal EvaluatePublicTransport(int distanceMeters)
    {
        if (distanceMeters <= 500) return 100m;
        if (distanceMeters <= 1000) return 85m;
        if (distanceMeters <= 2000) return 70m;
        return 50m;
    }

    private decimal EvaluateRentalYield(decimal yieldPercentage)
    {
        if (yieldPercentage >= 5.0m) return 100m;
        if (yieldPercentage >= 4.0m) return 85m;
        if (yieldPercentage >= 3.0m) return 70m;
        if (yieldPercentage >= 2.0m) return 50m;
        return 30m;
    }

    private decimal EvaluateCapitalGrowth(decimal growthPercentage)
    {
        if (growthPercentage >= 7.0m) return 100m;
        if (growthPercentage >= 5.0m) return 85m;
        if (growthPercentage >= 3.0m) return 70m;
        if (growthPercentage >= 1.0m) return 50m;
        return 30m;
    }

    private decimal EvaluateVacancyRate(decimal vacancyPercentage)
    {
        if (vacancyPercentage <= 2.0m) return 100m;
        if (vacancyPercentage <= 3.0m) return 85m;
        if (vacancyPercentage <= 5.0m) return 70m;
        if (vacancyPercentage <= 7.0m) return 50m;
        return 30m;
    }

    private decimal EvaluateLocalDemand(string demand)
    {
        return demand.ToLower() switch
        {
            "high" => 100m,
            "medium" => 70m,
            "low" => 40m,
            _ => 30m
        };
    }

    private decimal EvaluateStructuralSoundness(bool hasIssues, int ageYears)
    {
        if (hasIssues) return 30m;
        if (ageYears <= 10) return 100m;
        if (ageYears <= 20) return 85m;
        if (ageYears <= 40) return 70m;
        return 60m;
    }

    private decimal EvaluateMajorDefects(bool hasDefects)
    {
        return hasDefects ? 0m : 100m;
    }

    private decimal EvaluateMaintenance(string level)
    {
        return level.ToLower() switch
        {
            "minimal" => 100m,
            "moderate" => 70m,
            "extensive" => 40m,
            _ => 50m
        };
    }

    private decimal EvaluateCompliance(bool meetsCodes, bool hasCertificates)
    {
        if (meetsCodes && hasCertificates) return 100m;
        if (meetsCodes || hasCertificates) return 60m;
        return 20m;
    }

    private decimal EvaluateTenantQuality(bool longTerm, bool reliablePayment)
    {
        if (longTerm && reliablePayment) return 100m;
        if (longTerm || reliablePayment) return 70m;
        return 40m;
    }

    private decimal EvaluateLeaseStatus(int remainingMonths)
    {
        if (remainingMonths >= 12) return 100m;
        if (remainingMonths >= 6) return 75m;
        if (remainingMonths >= 3) return 50m;
        return 30m;
    }

    private decimal EvaluateRentalConsistency(bool hasConsistency)
    {
        return hasConsistency ? 100m : 50m;
    }

    private decimal EvaluateCashFlowCoverage(decimal ratio)
    {
        if (ratio >= 1.3m) return 100m;
        if (ratio >= 1.2m) return 85m;
        if (ratio >= 1.1m) return 70m;
        if (ratio >= 1.0m) return 55m;
        return 30m;
    }

    private decimal EvaluateLoanServiceability(bool meetsRequirements)
    {
        return meetsRequirements ? 100m : 30m;
    }

    private decimal EvaluateEquityBuffer(decimal ltvRatio)
    {
        if (ltvRatio <= 60m) return 100m;
        if (ltvRatio <= 70m) return 85m;
        if (ltvRatio <= 80m) return 70m;
        if (ltvRatio <= 90m) return 50m;
        return 30m;
    }

    private decimal EvaluateInsuranceCosts(decimal annualCost)
    {
        if (annualCost <= 1500m) return 100m;
        if (annualCost <= 2500m) return 80m;
        if (annualCost <= 3500m) return 60m;
        return 40m;
    }

    private decimal EvaluateCrossCollateralSuitability(bool suitable)
    {
        return suitable ? 100m : 40m;
    }

    private decimal EvaluateBorrowingCapacity(decimal equity)
    {
        if (equity >= 200000m) return 100m;
        if (equity >= 150000m) return 85m;
        if (equity >= 100000m) return 70m;
        if (equity >= 50000m) return 55m;
        return 40m;
    }

    private decimal EvaluateRefinanceEligibility(bool eligible)
    {
        return eligible ? 100m : 50m;
    }

    private decimal EvaluateSaleHistory(bool stable, int yearsSinceSale)
    {
        if (stable && yearsSinceSale >= 2) return 100m;
        if (stable) return 80m;
        if (yearsSinceSale >= 2) return 60m;
        return 40m;
    }

    private decimal EvaluateMarketActivity(int daysOnMarket)
    {
        if (daysOnMarket <= 30) return 100m;
        if (daysOnMarket <= 60) return 80m;
        if (daysOnMarket <= 90) return 60m;
        return 40m;
    }

    private decimal EvaluateComparableSales(bool hasStrong)
    {
        return hasStrong ? 100m : 50m;
    }

    private decimal EvaluateUniqueness(bool isUnique)
    {
        // For anchor properties, standard is better than unique
        return isUnique ? 70m : 100m;
    }

    private decimal EvaluateLenderAcceptance(bool accepted)
    {
        return accepted ? 100m : 30m;
    }

    private decimal EvaluateRiskRating(string rating)
    {
        return rating.ToLower() switch
        {
            "low" => 100m,
            "medium" => 70m,
            "high" => 30m,
            _ => 50m
        };
    }

    private decimal EvaluateDevelopmentRisk(bool hasRisk)
    {
        return hasRisk ? 30m : 100m;
    }

    private decimal EvaluatePortfolioDiversity(bool fits)
    {
        return fits ? 100m : 50m;
    }

    private decimal EvaluateLongTermViability(bool viable)
    {
        return viable ? 100m : 40m;
    }

    private string GenerateAnalysisResult(decimal score)
    {
        return score switch
        {
            >= 80 => "Excellent - Strong anchor property",
            >= 60 => "Good - Suitable anchor property",
            >= 40 => "Fair - Moderate anchor potential",
            _ => "Poor - Low anchor potential"
        };
    }
}
