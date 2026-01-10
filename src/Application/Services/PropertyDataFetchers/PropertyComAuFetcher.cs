using System.Text.Json;
using System.Text.RegularExpressions;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class PropertyComAuFetcher : IPropertyDataFetcher
{
    private readonly ChatGptUrlDiscoveryService _chatGptService;

    public int Priority => 3;
    public string SourceName => "Property.com.au";

    public PropertyComAuFetcher(ChatGptUrlDiscoveryService chatGptService)
    {
        _chatGptService = chatGptService;
    }

    public async Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress)
    {
        try
        {
            Console.WriteLine($"Fetching property data for: {propertyAddress} via OpenAI");

            // Ask OpenAI directly for property data based on the address
            var propertyData = await _chatGptService.GetPropertyDataAsync(propertyAddress);

            if (propertyData != null)
            {
                Console.WriteLine($"OpenAI successfully provided property data for: {propertyAddress}");
                return propertyData;
            }

            Console.WriteLine($"OpenAI could not provide property data for: {propertyAddress}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from Property.com.au: {ex.Message}");
            return null;
        }
    }

    public async Task<PropertyDataDto?> FetchPropertyDataFromUrlAsync(string propertyUrl)
    {
        try
        {
            Console.WriteLine($"Fetching property data directly from URL: {propertyUrl}");

            // Extract address from URL and use ChatGPT to get property data
            var address = ExtractAddressFromUrl(propertyUrl);
            if (!string.IsNullOrEmpty(address))
            {
                var propertyData = await _chatGptService.GetPropertyDataAsync(address);
                if (propertyData != null)
                {
                    Console.WriteLine($"OpenAI successfully provided property data for URL: {propertyUrl}");
                    return propertyData;
                }
            }

            Console.WriteLine($"Could not fetch property data from URL: {propertyUrl}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from URL {propertyUrl}: {ex.Message}");
            return null;
        }
    }

    private static string? ExtractAddressFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.Trim('/');

            // Property.com.au URLs typically look like: /property/123-example-street-suburb-state-1234567
            if (path.StartsWith("property/", StringComparison.OrdinalIgnoreCase))
                path = path["property/".Length..];

            var parts = path.Split('-');
            if (parts.Length < 3) return null;

            var address = string.Join(" ", parts).Replace("  ", " ");
            return address;
        }
        catch
        {
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
