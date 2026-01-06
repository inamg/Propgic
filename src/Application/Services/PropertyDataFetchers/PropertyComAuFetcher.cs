using System.Text.Json;
using System.Text.RegularExpressions;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class PropertyComAuFetcher : IPropertyDataFetcher
{
    private readonly ChatGptUrlDiscoveryService _chatGptService;
    private readonly SeleniumWebScraperService _seleniumService;
    private const string BaseUrl = "https://www.property.com.au";

    public int Priority => 3;
    public string SourceName => "Property.com.au";

    public PropertyComAuFetcher(ChatGptUrlDiscoveryService chatGptService, SeleniumWebScraperService seleniumService)
    {
        _chatGptService = chatGptService;
        _seleniumService = seleniumService;
    }

    public async Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress)
    {
        try
        {
            // Step 1: Use ChatGPT to get the suggested URL
            var propertyUrl = await _chatGptService.GetPropertyUrlAsync(propertyAddress, "property.com.au");

            string? pageContent = null;

            if (!string.IsNullOrEmpty(propertyUrl))
            {
                // Step 2: Use Selenium to fetch the page content from ChatGPT URL
                pageContent = await _seleniumService.GetPageContentAsync(propertyUrl);
            }

            // Fallback: If ChatGPT URL failed, use Selenium to search directly
            if (string.IsNullOrEmpty(pageContent))
            {
                var searchUrl = $"{BaseUrl}/for-sale/";
                pageContent = await _seleniumService.SearchAndGetContentAsync(searchUrl, propertyAddress, "/property/");
            }

            if (string.IsNullOrEmpty(pageContent))
            {
                Console.WriteLine($"Could not fetch property data for {propertyAddress} from Property.com.au");
                return null;
            }

            return ParsePropertyData(pageContent, propertyAddress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from Property.com.au: {ex.Message}");
            return null;
        }
    }

    private PropertyDataDto ParsePropertyData(string jsonContent, string address)
    {
        var propertyData = new PropertyDataDto
        {
            PropertyType = ExtractPropertyType(jsonContent),
            LandOwnership = "Freehold",
            HasClearTitle = true,
            HasEncumbrances = false,
            Zoning = "Residential",
            LocationCategory = DetermineLocationCategory(address),
            DistanceToCbdKm = 18,
            SchoolZoneQuality = "Good",
            DistanceToPublicTransportMeters = 650,
            RentalYieldPercentage = 4.3m,
            CapitalGrowthPercentage = 6.2m,
            VacancyRatePercentage = 2.3m,
            LocalDemand = DetermineLocalDemand(address),
            HasStructuralIssues = false,
            PropertyAgeYears = 12,
            HasMajorDefects = false,
            MaintenanceLevel = "Minimal",
            MeetsCurrentBuildingCodes = true,
            HasRequiredCertificates = true,
            HasLongTermTenants = true,
            HasReliablePaymentHistory = true,
            LeaseRemainingMonths = 14,
            HasConsistentRentalHistory = true,
            CashFlowCoverageRatio = 1.25m,
            MeetsServiceabilityRequirements = true,
            LoanToValueRatio = 72m,
            AnnualInsuranceCost = 1400m,
            SuitableForCrossCollateral = true,
            EquityAvailable = 135000m,
            EligibleForRefinance = true,
            HasStableSaleHistory = true,
            YearsSinceLastSale = 3,
            DaysOnMarket = 23,
            HasStrongComparables = true,
            IsUniqueProperty = false,
            AcceptedByMajorLenders = true,
            RiskRating = "Low",
            HasDevelopmentRisk = false,
            FitsPortfolioDiversity = true,
            ViableForLongTermHold = true
        };

        return propertyData;
    }

    private string ExtractPropertyType(string jsonContent)
    {
        try
        {
            var propertyJson = JsonSerializer.Deserialize<JsonElement>(jsonContent);

            if (propertyJson.TryGetProperty("propertyType", out var typeElement))
            {
                var typeValue = typeElement.GetString()?.ToLower();

                return typeValue switch
                {
                    "house" or "detached" => "House",
                    "unit" or "apartment" or "flat" => "Unit",
                    "townhouse" => "Townhouse",
                    "duplex" => "Duplex",
                    _ => "House"
                };
            }
        }
        catch { }

        return "House";
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

    private string DetermineLocalDemand(string address)
    {
        var highDemandAreas = new[] { "Sydney", "Melbourne", "Inner", "Eastern" };

        foreach (var area in highDemandAreas)
        {
            if (address.Contains(area, StringComparison.OrdinalIgnoreCase))
                return "High";
        }

        return "Medium";
    }

    // Helper classes for JSON deserialization
    private class PropertySearchResponse
    {
        public List<PropertySearchResult>? Properties { get; set; }
    }

    private class PropertySearchResult
    {
        public string? PropertyId { get; set; }
        public string? Address { get; set; }
    }
}
