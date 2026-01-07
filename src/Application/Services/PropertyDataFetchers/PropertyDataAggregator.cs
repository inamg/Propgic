using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class PropertyDataAggregator
{
    private readonly IEnumerable<IPropertyDataFetcher> _fetchers;
    private readonly SeleniumWebScraperService _seleniumService;

    public PropertyDataAggregator(IEnumerable<IPropertyDataFetcher> fetchers, SeleniumWebScraperService seleniumService)
    {
        _fetchers = fetchers.OrderBy(f => f.Priority);
        _seleniumService = seleniumService;
    }

    public async Task<PropertyDataDto> FetchAndAggregateAsync(string propertyAddress)
    {
        var results = new List<PropertyDataDto>();

        // Try each fetcher in priority order
        foreach (var fetcher in _fetchers)
        {
            try
            {
                Console.WriteLine($"Fetching from {fetcher.SourceName}...");
                var data = await fetcher.FetchPropertyDataAsync(propertyAddress);

                if (data != null)
                {
                    Console.WriteLine($"Successfully fetched data from {fetcher.SourceName}");
                    results.Add(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with {fetcher.SourceName}: {ex.Message}");
            }
        }

        // If we got data from any source, aggregate it
        if (results.Any())
        {
            return AggregateResults(results);
        }

        // Fallback to default data
        Console.WriteLine("No data fetched from web sources, using default values");
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

            Console.WriteLine("Could not fetch data from URL, using default values");
            return GetDefaultPropertyData(propertyUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from URL: {ex.Message}");
            return GetDefaultPropertyData(propertyUrl);
        }
    }

    private PropertyDataDto AggregateResults(List<PropertyDataDto> results)
    {
        // Use the first result as base and fill in missing data from others
        var aggregated = results.First();

        // For numerical values, take the average
        if (results.Count > 1)
        {
            aggregated.RentalYieldPercentage = results.Average(r => r.RentalYieldPercentage);
            aggregated.CapitalGrowthPercentage = results.Average(r => r.CapitalGrowthPercentage);
            aggregated.VacancyRatePercentage = results.Average(r => r.VacancyRatePercentage);
            aggregated.PropertyAgeYears = (int)results.Average(r => r.PropertyAgeYears);
            aggregated.DistanceToCbdKm = (int)results.Average(r => r.DistanceToCbdKm);
            aggregated.DistanceToPublicTransportMeters = (int)results.Average(r => r.DistanceToPublicTransportMeters);
            aggregated.CashFlowCoverageRatio = results.Average(r => r.CashFlowCoverageRatio);
            aggregated.LoanToValueRatio = results.Average(r => r.LoanToValueRatio);
            aggregated.EquityAvailable = results.Average(r => r.EquityAvailable);
            aggregated.AnnualInsuranceCost = results.Average(r => r.AnnualInsuranceCost);
            aggregated.YearsSinceLastSale = (int)results.Average(r => r.YearsSinceLastSale);
            aggregated.DaysOnMarket = (int)results.Average(r => r.DaysOnMarket);

            // For string values, use majority vote or first non-default value
            aggregated.PropertyType = GetMostCommonValue(results.Select(r => r.PropertyType));
            aggregated.LandOwnership = GetMostCommonValue(results.Select(r => r.LandOwnership));
            aggregated.LocationCategory = GetMostCommonValue(results.Select(r => r.LocationCategory));
            aggregated.LocalDemand = GetMostCommonValue(results.Select(r => r.LocalDemand));
            aggregated.SchoolZoneQuality = GetMostCommonValue(results.Select(r => r.SchoolZoneQuality));
            aggregated.MaintenanceLevel = GetMostCommonValue(results.Select(r => r.MaintenanceLevel));
            aggregated.RiskRating = GetMostCommonValue(results.Select(r => r.RiskRating));

            // For boolean values, use majority vote
            aggregated.HasClearTitle = results.Count(r => r.HasClearTitle) > results.Count / 2;
            aggregated.HasEncumbrances = results.Count(r => r.HasEncumbrances) > results.Count / 2;
            aggregated.HasStructuralIssues = results.Count(r => r.HasStructuralIssues) > results.Count / 2;
            aggregated.HasMajorDefects = results.Count(r => r.HasMajorDefects) > results.Count / 2;
            aggregated.IsUniqueProperty = results.Count(r => r.IsUniqueProperty) > results.Count / 2;
        }

        return aggregated;
    }

    private string GetMostCommonValue(IEnumerable<string> values)
    {
        return values
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
