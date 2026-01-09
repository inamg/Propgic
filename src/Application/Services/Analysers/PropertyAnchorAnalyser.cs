using Propgic.Application.Interfaces;
using Propgic.Application.DTOs;
using Propgic.Application.Services.PropertyDataFetchers;
using Propgic.Domain.Entities;
using Propgic.Domain.Interfaces;

namespace Propgic.Application.Services.Analysers;

public class PropertyAnchorAnalyser : IPropertyAnalyser
{
    private readonly PropertyDataAggregator _dataAggregator;
    private readonly IRepository<PropertyData> _propertyDataRepository;
    private readonly IUnitOfWork _unitOfWork;

    public string AnalyserType => "PropertyAnchor";

    public PropertyAnchorAnalyser(
        PropertyDataAggregator dataAggregator,
        IRepository<PropertyData> propertyDataRepository,
        IUnitOfWork unitOfWork)
    {
        _dataAggregator = dataAggregator;
        _propertyDataRepository = propertyDataRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PropertyAnalysis> AnalyseAsync(PropertyAnalysis propertyAnalysis)
    {
        // Update status to InProgress
        propertyAnalysis.Status = "InProgress";

        try
        {
            // Fetch property data based on SourceType
            PropertyDataDto propertyDataDto;
            string dataSource;
            if (propertyAnalysis.SourceType == "Url")
            {
                propertyDataDto = await _dataAggregator.FetchFromUrlAsync(propertyAnalysis.PropertyAddress);
                dataSource = "URL";
            }
            else
            {
                propertyDataDto = await _dataAggregator.FetchAndAggregateAsync(propertyAnalysis.PropertyAddress);
                dataSource = "OpenAI";
            }

            // Save property data to database
            var propertyDataEntity = MapToPropertyDataEntity(propertyDataDto, propertyAnalysis.PropertyAddress, dataSource);
            if (propertyAnalysis.SourceType == "Url")
            {
                propertyDataEntity.PropertyUrl = propertyAnalysis.PropertyAddress;
            }
            await _propertyDataRepository.AddAsync(propertyDataEntity);
            await _unitOfWork.SaveChangesAsync();

            // Perform analysis
            var score = CalculateAnchorScore(propertyDataDto);
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

    private static PropertyData MapToPropertyDataEntity(PropertyDataDto dto, string propertyAddress, string dataSource)
    {
        return new PropertyData
        {
            Id = Guid.NewGuid(),
            PropertyAddress = propertyAddress,
            CreatedAt = DateTime.UtcNow,
            DataSource = dataSource,
            PropertyType = dto.PropertyType,
            LandOwnership = dto.LandOwnership,
            HasClearTitle = dto.HasClearTitle,
            HasEncumbrances = dto.HasEncumbrances,
            Zoning = dto.Zoning,
            LocationCategory = dto.LocationCategory,
            DistanceToCbdKm = dto.DistanceToCbdKm,
            SchoolZoneQuality = dto.SchoolZoneQuality,
            DistanceToPublicTransportMeters = dto.DistanceToPublicTransportMeters,
            RentalYieldPercentage = dto.RentalYieldPercentage,
            CapitalGrowthPercentage = dto.CapitalGrowthPercentage,
            VacancyRatePercentage = dto.VacancyRatePercentage,
            LocalDemand = dto.LocalDemand,
            HasStructuralIssues = dto.HasStructuralIssues,
            PropertyAgeYears = dto.PropertyAgeYears,
            HasMajorDefects = dto.HasMajorDefects,
            MaintenanceLevel = dto.MaintenanceLevel,
            MeetsCurrentBuildingCodes = dto.MeetsCurrentBuildingCodes,
            HasRequiredCertificates = dto.HasRequiredCertificates,
            HasLongTermTenants = dto.HasLongTermTenants,
            HasReliablePaymentHistory = dto.HasReliablePaymentHistory,
            LeaseRemainingMonths = dto.LeaseRemainingMonths,
            HasConsistentRentalHistory = dto.HasConsistentRentalHistory,
            CashFlowCoverageRatio = dto.CashFlowCoverageRatio,
            MeetsServiceabilityRequirements = dto.MeetsServiceabilityRequirements,
            LoanToValueRatio = dto.LoanToValueRatio,
            AnnualInsuranceCost = dto.AnnualInsuranceCost,
            SuitableForCrossCollateral = dto.SuitableForCrossCollateral,
            EquityAvailable = dto.EquityAvailable,
            EligibleForRefinance = dto.EligibleForRefinance,
            HasStableSaleHistory = dto.HasStableSaleHistory,
            YearsSinceLastSale = dto.YearsSinceLastSale,
            DaysOnMarket = dto.DaysOnMarket,
            HasStrongComparables = dto.HasStrongComparables,
            IsUniqueProperty = dto.IsUniqueProperty,
            AcceptedByMajorLenders = dto.AcceptedByMajorLenders,
            RiskRating = dto.RiskRating,
            HasDevelopmentRisk = dto.HasDevelopmentRisk,
            FitsPortfolioDiversity = dto.FitsPortfolioDiversity,
            ViableForLongTermHold = dto.ViableForLongTermHold
        };
    }

    private decimal CalculateAnchorScore(PropertyDataDto propertyData)
    {
        decimal totalScore = 0m;
        decimal totalWeight = 0m;

        // 1. Property type (6%)
        if (propertyData.PropertyType != null)
        {
            totalScore += EvaluatePropertyType(propertyData.PropertyType) * 0.06m;
            totalWeight += 0.06m;
        }

        // 2. Land ownership (5%)
        if (propertyData.LandOwnership != null)
        {
            totalScore += EvaluateLandOwnership(propertyData.LandOwnership) * 0.05m;
            totalWeight += 0.05m;
        }

        // 3. Title clarity (8%)
        if (propertyData.HasClearTitle.HasValue && propertyData.HasEncumbrances.HasValue)
        {
            totalScore += EvaluateTitleClarity(propertyData.HasClearTitle.Value, propertyData.HasEncumbrances.Value) * 0.08m;
            totalWeight += 0.08m;
        }

        // 4. Zoning (4%)
        if (propertyData.Zoning != null)
        {
            totalScore += EvaluateZoning(propertyData.Zoning) * 0.04m;
            totalWeight += 0.04m;
        }

        // 5. Location category (5%)
        if (propertyData.LocationCategory != null)
        {
            totalScore += EvaluateLocationCategory(propertyData.LocationCategory) * 0.05m;
            totalWeight += 0.05m;
        }

        // 6. Proximity to CBD (3%)
        if (propertyData.DistanceToCbdKm.HasValue)
        {
            totalScore += EvaluateProximityToCbd(propertyData.DistanceToCbdKm.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 7. School zone quality (4%)
        if (propertyData.SchoolZoneQuality != null)
        {
            totalScore += EvaluateSchoolZone(propertyData.SchoolZoneQuality) * 0.04m;
            totalWeight += 0.04m;
        }

        // 8. Public transport (3%)
        if (propertyData.DistanceToPublicTransportMeters.HasValue)
        {
            totalScore += EvaluatePublicTransport(propertyData.DistanceToPublicTransportMeters.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 9. Rental yield (7%)
        if (propertyData.RentalYieldPercentage.HasValue)
        {
            totalScore += EvaluateRentalYield(propertyData.RentalYieldPercentage.Value) * 0.07m;
            totalWeight += 0.07m;
        }

        // 10. Capital growth trend (6%)
        if (propertyData.CapitalGrowthPercentage.HasValue)
        {
            totalScore += EvaluateCapitalGrowth(propertyData.CapitalGrowthPercentage.Value) * 0.06m;
            totalWeight += 0.06m;
        }

        // 11. Vacancy rate (4%)
        if (propertyData.VacancyRatePercentage.HasValue)
        {
            totalScore += EvaluateVacancyRate(propertyData.VacancyRatePercentage.Value) * 0.04m;
            totalWeight += 0.04m;
        }

        // 12. Local area demand (5%)
        if (propertyData.LocalDemand != null)
        {
            totalScore += EvaluateLocalDemand(propertyData.LocalDemand) * 0.05m;
            totalWeight += 0.05m;
        }

        // 13. Structural soundness (6%)
        if (propertyData.HasStructuralIssues.HasValue && propertyData.PropertyAgeYears.HasValue)
        {
            totalScore += EvaluateStructuralSoundness(propertyData.HasStructuralIssues.Value, propertyData.PropertyAgeYears.Value) * 0.06m;
            totalWeight += 0.06m;
        }

        // 14. Major defects (5%)
        if (propertyData.HasMajorDefects.HasValue)
        {
            totalScore += EvaluateMajorDefects(propertyData.HasMajorDefects.Value) * 0.05m;
            totalWeight += 0.05m;
        }

        // 15. Maintenance required (3%)
        if (propertyData.MaintenanceLevel != null)
        {
            totalScore += EvaluateMaintenance(propertyData.MaintenanceLevel) * 0.03m;
            totalWeight += 0.03m;
        }

        // 16. Compliance (4%)
        if (propertyData.MeetsCurrentBuildingCodes.HasValue && propertyData.HasRequiredCertificates.HasValue)
        {
            totalScore += EvaluateCompliance(propertyData.MeetsCurrentBuildingCodes.Value, propertyData.HasRequiredCertificates.Value) * 0.04m;
            totalWeight += 0.04m;
        }

        // 17. Tenant quality (3%)
        if (propertyData.HasLongTermTenants.HasValue && propertyData.HasReliablePaymentHistory.HasValue)
        {
            totalScore += EvaluateTenantQuality(propertyData.HasLongTermTenants.Value, propertyData.HasReliablePaymentHistory.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 18. Lease status (3%)
        if (propertyData.LeaseRemainingMonths.HasValue)
        {
            totalScore += EvaluateLeaseStatus(propertyData.LeaseRemainingMonths.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 19. Rental consistency (2%)
        if (propertyData.HasConsistentRentalHistory.HasValue)
        {
            totalScore += EvaluateRentalConsistency(propertyData.HasConsistentRentalHistory.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 20. Cash flow coverage (5%)
        if (propertyData.CashFlowCoverageRatio.HasValue)
        {
            totalScore += EvaluateCashFlowCoverage(propertyData.CashFlowCoverageRatio.Value) * 0.05m;
            totalWeight += 0.05m;
        }

        // 21. Loan serviceability (4%)
        if (propertyData.MeetsServiceabilityRequirements.HasValue)
        {
            totalScore += EvaluateLoanServiceability(propertyData.MeetsServiceabilityRequirements.Value) * 0.04m;
            totalWeight += 0.04m;
        }

        // 22. Equity buffer (3%)
        if (propertyData.LoanToValueRatio.HasValue)
        {
            totalScore += EvaluateEquityBuffer(propertyData.LoanToValueRatio.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 23. Insurance costs (2%)
        if (propertyData.AnnualInsuranceCost.HasValue)
        {
            totalScore += EvaluateInsuranceCosts(propertyData.AnnualInsuranceCost.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 24. Cross-collateral suitability (4%)
        if (propertyData.SuitableForCrossCollateral.HasValue)
        {
            totalScore += EvaluateCrossCollateralSuitability(propertyData.SuitableForCrossCollateral.Value) * 0.04m;
            totalWeight += 0.04m;
        }

        // 25. Borrowing capacity boost (3%)
        if (propertyData.EquityAvailable.HasValue)
        {
            totalScore += EvaluateBorrowingCapacity(propertyData.EquityAvailable.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 26. Refinance eligibility (2%)
        if (propertyData.EligibleForRefinance.HasValue)
        {
            totalScore += EvaluateRefinanceEligibility(propertyData.EligibleForRefinance.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 27. Sale history (2%)
        if (propertyData.HasStableSaleHistory.HasValue && propertyData.YearsSinceLastSale.HasValue)
        {
            totalScore += EvaluateSaleHistory(propertyData.HasStableSaleHistory.Value, propertyData.YearsSinceLastSale.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 28. Market activity (2%)
        if (propertyData.DaysOnMarket.HasValue)
        {
            totalScore += EvaluateMarketActivity(propertyData.DaysOnMarket.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 29. Comparable sales (2%)
        if (propertyData.HasStrongComparables.HasValue)
        {
            totalScore += EvaluateComparableSales(propertyData.HasStrongComparables.Value) * 0.02m;
            totalWeight += 0.02m;
        }

        // 30. Uniqueness/scarcity (1%)
        if (propertyData.IsUniqueProperty.HasValue)
        {
            totalScore += EvaluateUniqueness(propertyData.IsUniqueProperty.Value) * 0.01m;
            totalWeight += 0.01m;
        }

        // 31. Lender acceptance (3%)
        if (propertyData.AcceptedByMajorLenders.HasValue)
        {
            totalScore += EvaluateLenderAcceptance(propertyData.AcceptedByMajorLenders.Value) * 0.03m;
            totalWeight += 0.03m;
        }

        // 32. Risk rating (2%)
        if (propertyData.RiskRating != null)
        {
            totalScore += EvaluateRiskRating(propertyData.RiskRating) * 0.02m;
            totalWeight += 0.02m;
        }

        // 33. Future development risk (1%)
        if (propertyData.HasDevelopmentRisk.HasValue)
        {
            totalScore += EvaluateDevelopmentRisk(propertyData.HasDevelopmentRisk.Value) * 0.01m;
            totalWeight += 0.01m;
        }

        // 34. Portfolio diversity fit (1%)
        if (propertyData.FitsPortfolioDiversity.HasValue)
        {
            totalScore += EvaluatePortfolioDiversity(propertyData.FitsPortfolioDiversity.Value) * 0.01m;
            totalWeight += 0.01m;
        }

        // 35. Long-term hold viability (1%)
        if (propertyData.ViableForLongTermHold.HasValue)
        {
            totalScore += EvaluateLongTermViability(propertyData.ViableForLongTermHold.Value) * 0.01m;
            totalWeight += 0.01m;
        }

        // Normalize score based on available data
        if (totalWeight > 0)
        {
            return Math.Round(totalScore / totalWeight * 100m, 2);
        }

        return 0m;
    }

    // Evaluation methods for each attribute (return score 0-100)
    private static decimal EvaluatePropertyType(string propertyType)
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

    private static decimal EvaluateLandOwnership(string landOwnership)
    {
        return landOwnership.ToLower() switch
        {
            "freehold" => 100m,
            "strata" => 75m,
            "leasehold" => 50m,
            _ => 40m
        };
    }

    private static decimal EvaluateTitleClarity(bool hasClearTitle, bool hasEncumbrances)
    {
        if (hasClearTitle && !hasEncumbrances) return 100m;
        if (hasClearTitle && hasEncumbrances) return 70m;
        if (!hasClearTitle && !hasEncumbrances) return 60m;
        return 30m;
    }

    private static decimal EvaluateZoning(string zoning)
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

    private static decimal EvaluateLocationCategory(string locationCategory)
    {
        return locationCategory.ToLower() switch
        {
            "metro" or "metropolitan" => 100m,
            "regional" => 75m,
            "rural" => 50m,
            _ => 40m
        };
    }

    private static decimal EvaluateProximityToCbd(int distanceKm)
    {
        if (distanceKm <= 10) return 100m;
        if (distanceKm <= 20) return 85m;
        if (distanceKm <= 30) return 70m;
        if (distanceKm <= 50) return 55m;
        return 40m;
    }

    private static decimal EvaluateSchoolZone(string quality)
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

    private static decimal EvaluatePublicTransport(int distanceMeters)
    {
        if (distanceMeters <= 500) return 100m;
        if (distanceMeters <= 1000) return 85m;
        if (distanceMeters <= 2000) return 70m;
        return 50m;
    }

    private static decimal EvaluateRentalYield(decimal yieldPercentage)
    {
        if (yieldPercentage >= 5.0m) return 100m;
        if (yieldPercentage >= 4.0m) return 85m;
        if (yieldPercentage >= 3.0m) return 70m;
        if (yieldPercentage >= 2.0m) return 50m;
        return 30m;
    }

    private static decimal EvaluateCapitalGrowth(decimal growthPercentage)
    {
        if (growthPercentage >= 7.0m) return 100m;
        if (growthPercentage >= 5.0m) return 85m;
        if (growthPercentage >= 3.0m) return 70m;
        if (growthPercentage >= 1.0m) return 50m;
        return 30m;
    }

    private static decimal EvaluateVacancyRate(decimal vacancyPercentage)
    {
        if (vacancyPercentage <= 2.0m) return 100m;
        if (vacancyPercentage <= 3.0m) return 85m;
        if (vacancyPercentage <= 5.0m) return 70m;
        if (vacancyPercentage <= 7.0m) return 50m;
        return 30m;
    }

    private static decimal EvaluateLocalDemand(string demand)
    {
        return demand.ToLower() switch
        {
            "high" => 100m,
            "medium" => 70m,
            "low" => 40m,
            _ => 30m
        };
    }

    private static decimal EvaluateStructuralSoundness(bool hasIssues, int ageYears)
    {
        if (hasIssues) return 30m;
        if (ageYears <= 10) return 100m;
        if (ageYears <= 20) return 85m;
        if (ageYears <= 40) return 70m;
        return 60m;
    }

    private static decimal EvaluateMajorDefects(bool hasDefects)
    {
        return hasDefects ? 0m : 100m;
    }

    private static decimal EvaluateMaintenance(string level)
    {
        return level.ToLower() switch
        {
            "minimal" => 100m,
            "moderate" => 70m,
            "extensive" => 40m,
            _ => 50m
        };
    }

    private static decimal EvaluateCompliance(bool meetsCodes, bool hasCertificates)
    {
        if (meetsCodes && hasCertificates) return 100m;
        if (meetsCodes || hasCertificates) return 60m;
        return 20m;
    }

    private static decimal EvaluateTenantQuality(bool longTerm, bool reliablePayment)
    {
        if (longTerm && reliablePayment) return 100m;
        if (longTerm || reliablePayment) return 70m;
        return 40m;
    }

    private static decimal EvaluateLeaseStatus(int remainingMonths)
    {
        if (remainingMonths >= 12) return 100m;
        if (remainingMonths >= 6) return 75m;
        if (remainingMonths >= 3) return 50m;
        return 30m;
    }

    private static decimal EvaluateRentalConsistency(bool hasConsistency)
    {
        return hasConsistency ? 100m : 50m;
    }

    private static decimal EvaluateCashFlowCoverage(decimal ratio)
    {
        if (ratio >= 1.3m) return 100m;
        if (ratio >= 1.2m) return 85m;
        if (ratio >= 1.1m) return 70m;
        if (ratio >= 1.0m) return 55m;
        return 30m;
    }

    private static decimal EvaluateLoanServiceability(bool meetsRequirements)
    {
        return meetsRequirements ? 100m : 30m;
    }

    private static decimal EvaluateEquityBuffer(decimal ltvRatio)
    {
        if (ltvRatio <= 60m) return 100m;
        if (ltvRatio <= 70m) return 85m;
        if (ltvRatio <= 80m) return 70m;
        if (ltvRatio <= 90m) return 50m;
        return 30m;
    }

    private static decimal EvaluateInsuranceCosts(decimal annualCost)
    {
        if (annualCost <= 1500m) return 100m;
        if (annualCost <= 2500m) return 80m;
        if (annualCost <= 3500m) return 60m;
        return 40m;
    }

    private static decimal EvaluateCrossCollateralSuitability(bool suitable)
    {
        return suitable ? 100m : 40m;
    }

    private static decimal EvaluateBorrowingCapacity(decimal equity)
    {
        if (equity >= 200000m) return 100m;
        if (equity >= 150000m) return 85m;
        if (equity >= 100000m) return 70m;
        if (equity >= 50000m) return 55m;
        return 40m;
    }

    private static decimal EvaluateRefinanceEligibility(bool eligible)
    {
        return eligible ? 100m : 50m;
    }

    private static decimal EvaluateSaleHistory(bool stable, int yearsSinceSale)
    {
        if (stable && yearsSinceSale >= 2) return 100m;
        if (stable) return 80m;
        if (yearsSinceSale >= 2) return 60m;
        return 40m;
    }

    private static decimal EvaluateMarketActivity(int daysOnMarket)
    {
        if (daysOnMarket <= 30) return 100m;
        if (daysOnMarket <= 60) return 80m;
        if (daysOnMarket <= 90) return 60m;
        return 40m;
    }

    private static decimal EvaluateComparableSales(bool hasStrong)
    {
        return hasStrong ? 100m : 50m;
    }

    private static decimal EvaluateUniqueness(bool isUnique)
    {
        // For anchor properties, standard is better than unique
        return isUnique ? 70m : 100m;
    }

    private static decimal EvaluateLenderAcceptance(bool accepted)
    {
        return accepted ? 100m : 30m;
    }

    private static decimal EvaluateRiskRating(string rating)
    {
        return rating.ToLower() switch
        {
            "low" => 100m,
            "medium" => 70m,
            "high" => 30m,
            _ => 50m
        };
    }

    private static decimal EvaluateDevelopmentRisk(bool hasRisk)
    {
        return hasRisk ? 30m : 100m;
    }

    private static decimal EvaluatePortfolioDiversity(bool fits)
    {
        return fits ? 100m : 50m;
    }

    private static decimal EvaluateLongTermViability(bool viable)
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
