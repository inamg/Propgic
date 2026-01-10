using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class PropertyDataAggregator
{
    private readonly IEnumerable<IPropertyDataFetcher> _fetchers;

    public PropertyDataAggregator(IEnumerable<IPropertyDataFetcher> fetchers)
    {
        _fetchers = fetchers.OrderBy(f => f.Priority);
    }

    public async Task<PropertyDataDto> FetchAndAggregateAsync(string propertyAddress)
    {
        // For address-based searches, use OpenAI fetcher directly
        var openAiFetcher = _fetchers.FirstOrDefault(f => f.SourceName == "OpenAI");

        if (openAiFetcher != null)
        {
            try
            {
                Console.WriteLine($"Fetching from OpenAI for address: {propertyAddress}");
                var data = await openAiFetcher.FetchPropertyDataAsync(propertyAddress);

                if (data != null)
                {
                    Console.WriteLine($"Successfully fetched data from OpenAI");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with OpenAI: {ex.Message}");
            }
        }

        // Fallback to default data if OpenAI fails
        Console.WriteLine("OpenAI fetch failed, using default values");
        return GetDefaultPropertyData(propertyAddress);
    }

    public async Task<PropertyDataDto> FetchFromUrlAsync(string propertyUrl)
    {
        try
        {
            Console.WriteLine($"Fetching property data directly from URL: {propertyUrl}");

            // Determine which fetcher to use based on the URL
            IPropertyDataFetcher? selectedFetcher = null;

            if (propertyUrl.Contains("domain.com.au", StringComparison.OrdinalIgnoreCase))
            {
                selectedFetcher = _fetchers.FirstOrDefault(f => f.SourceName.Contains("Domain"));
            }
            else if (propertyUrl.Contains("realestate.com.au", StringComparison.OrdinalIgnoreCase))
            {
                selectedFetcher = _fetchers.FirstOrDefault(f => f.SourceName.Contains("RealEstate"));
            }
            else if (propertyUrl.Contains("property.com.au", StringComparison.OrdinalIgnoreCase))
            {
                selectedFetcher = _fetchers.FirstOrDefault(f => f.SourceName.Contains("Property.com.au"));
            }

            if (selectedFetcher != null)
            {
                // Use the new interface method to fetch directly from URL
                var propertyData = await selectedFetcher.FetchPropertyDataFromUrlAsync(propertyUrl);

                if (propertyData != null)
                {
                    Console.WriteLine($"Successfully fetched and parsed data from {selectedFetcher.SourceName}");
                    return propertyData;
                }
            }

            // Fallback to OpenAI fetcher for URL-based searches
            var openAiFetcher = _fetchers.FirstOrDefault(f => f.SourceName == "OpenAI");
            if (openAiFetcher != null)
            {
                Console.WriteLine("Falling back to OpenAI for URL-based fetch");
                var openAiData = await openAiFetcher.FetchPropertyDataFromUrlAsync(propertyUrl);
                if (openAiData != null)
                {
                    Console.WriteLine("Successfully fetched data from OpenAI");
                    return openAiData;
                }
            }

            Console.WriteLine("Could not fetch data from URL, using default values");
            return GetDefaultPropertyData(propertyUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from URL: {ex.Message}");
            return GetDefaultPropertyData(propertyUrl);
        }
    }

    private static PropertyDataDto AggregateResults(List<PropertyDataDto> results)
    {
        // Use the first result as base and fill in missing data from others
        var aggregated = results.First();

        // For numerical values, take the average of non-null values
        if (results.Count > 1)
        {
            aggregated.RentalYieldPercentage = AverageNullable(results.Select(r => r.RentalYieldPercentage));
            aggregated.CapitalGrowthPercentage = AverageNullable(results.Select(r => r.CapitalGrowthPercentage));
            aggregated.VacancyRatePercentage = AverageNullable(results.Select(r => r.VacancyRatePercentage));
            aggregated.PropertyAgeYears = AverageNullableInt(results.Select(r => r.PropertyAgeYears));
            aggregated.DistanceToCbdKm = AverageNullableInt(results.Select(r => r.DistanceToCbdKm));
            aggregated.DistanceToPublicTransportMeters = AverageNullableInt(results.Select(r => r.DistanceToPublicTransportMeters));
            aggregated.CashFlowCoverageRatio = AverageNullable(results.Select(r => r.CashFlowCoverageRatio));
            aggregated.LoanToValueRatio = AverageNullable(results.Select(r => r.LoanToValueRatio));
            aggregated.EquityAvailable = AverageNullable(results.Select(r => r.EquityAvailable));
            aggregated.AnnualInsuranceCost = AverageNullable(results.Select(r => r.AnnualInsuranceCost));
            aggregated.YearsSinceLastSale = AverageNullableInt(results.Select(r => r.YearsSinceLastSale));
            aggregated.DaysOnMarket = AverageNullableInt(results.Select(r => r.DaysOnMarket));

            // For string values, use majority vote or first non-null value
            aggregated.PropertyType = GetMostCommonValue(results.Select(r => r.PropertyType));
            aggregated.LandOwnership = GetMostCommonValue(results.Select(r => r.LandOwnership));
            aggregated.LocationCategory = GetMostCommonValue(results.Select(r => r.LocationCategory));
            aggregated.LocalDemand = GetMostCommonValue(results.Select(r => r.LocalDemand));
            aggregated.SchoolZoneQuality = GetMostCommonValue(results.Select(r => r.SchoolZoneQuality));
            aggregated.MaintenanceLevel = GetMostCommonValue(results.Select(r => r.MaintenanceLevel));
            aggregated.RiskRating = GetMostCommonValue(results.Select(r => r.RiskRating));

            // For boolean values, use majority vote of non-null values
            aggregated.HasClearTitle = MajorityVote(results.Select(r => r.HasClearTitle));
            aggregated.HasEncumbrances = MajorityVote(results.Select(r => r.HasEncumbrances));
            aggregated.HasStructuralIssues = MajorityVote(results.Select(r => r.HasStructuralIssues));
            aggregated.HasMajorDefects = MajorityVote(results.Select(r => r.HasMajorDefects));
            aggregated.IsUniqueProperty = MajorityVote(results.Select(r => r.IsUniqueProperty));
        }

        return aggregated;
    }

    private static decimal? AverageNullable(IEnumerable<decimal?> values)
    {
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return nonNullValues.Count != 0 ? nonNullValues.Average() : null;
    }

    private static int? AverageNullableInt(IEnumerable<int?> values)
    {
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return nonNullValues.Count != 0 ? (int)nonNullValues.Average() : null;
    }

    private static bool? MajorityVote(IEnumerable<bool?> values)
    {
        var nonNullValues = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        if (nonNullValues.Count == 0) return null;
        return nonNullValues.Count(v => v) > nonNullValues.Count / 2;
    }

    private static string? GetMostCommonValue(IEnumerable<string?> values)
    {
        var nonNullValues = values.Where(v => v != null).ToList();
        if (nonNullValues.Count == 0) return null;

        return nonNullValues
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private PropertyDataDto GetDefaultPropertyData(string address)
    {
        return new PropertyDataDto
        {
            PropertyType = "House",
            LandOwnership = "Freehold",
            HasClearTitle = true,
            HasEncumbrances = false,
            Zoning = "Residential",
            LocationCategory = DetermineLocationCategory(address),
            DistanceToCbdKm = 20,
            SchoolZoneQuality = "Good",
            DistanceToPublicTransportMeters = 700,
            RentalYieldPercentage = 4.0m,
            CapitalGrowthPercentage = 5.5m,
            VacancyRatePercentage = 2.5m,
            LocalDemand = "Medium",
            HasStructuralIssues = false,
            PropertyAgeYears = 15,
            HasMajorDefects = false,
            MaintenanceLevel = "Minimal",
            MeetsCurrentBuildingCodes = true,
            HasRequiredCertificates = true,
            HasLongTermTenants = true,
            HasReliablePaymentHistory = true,
            LeaseRemainingMonths = 12,
            HasConsistentRentalHistory = true,
            CashFlowCoverageRatio = 1.2m,
            MeetsServiceabilityRequirements = true,
            LoanToValueRatio = 75m,
            AnnualInsuranceCost = 1500m,
            SuitableForCrossCollateral = true,
            EquityAvailable = 100000m,
            EligibleForRefinance = true,
            HasStableSaleHistory = true,
            YearsSinceLastSale = 3,
            DaysOnMarket = 30,
            HasStrongComparables = true,
            IsUniqueProperty = false,
            AcceptedByMajorLenders = true,
            RiskRating = "Low",
            HasDevelopmentRisk = false,
            FitsPortfolioDiversity = true,
            ViableForLongTermHold = true
        };
    }

    private string DetermineLocationCategory(string address)
    {
        var metroCities = new[] { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Canberra" };

        foreach (var city in metroCities)
        {
            if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                return "Metro";
        }

        return "Regional";
    }
}
