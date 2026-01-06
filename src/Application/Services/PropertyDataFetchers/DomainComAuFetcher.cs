using System.Text.Json;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class DomainComAuFetcher : IPropertyDataFetcher
{
    private readonly ChatGptUrlDiscoveryService _chatGptService;
    private readonly SeleniumWebScraperService _seleniumService;
    private const string BaseUrl = "https://www.domain.com.au";

    public int Priority => 1;
    public string SourceName => "Domain.com.au";

    public DomainComAuFetcher(ChatGptUrlDiscoveryService chatGptService, SeleniumWebScraperService seleniumService)
    {
        _chatGptService = chatGptService;
        _seleniumService = seleniumService;
    }

    public async Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress)
    {
        try
        {
            // Step 1: Use ChatGPT to get the suggested URL
            var domainUrl = await _chatGptService.GetPropertyUrlAsync(propertyAddress, "domain.com.au");

            string? pageContent = null;

            if (!string.IsNullOrEmpty(domainUrl))
            {
                // Step 2: Use Selenium to fetch the page content from ChatGPT URL
                pageContent = await _seleniumService.GetPageContentAsync(domainUrl);
            }

            // Fallback: If ChatGPT URL failed, use Selenium to search directly
            if (string.IsNullOrEmpty(pageContent))
            {
                var searchUrl = $"{BaseUrl}/sale/";
                pageContent = await _seleniumService.SearchAndGetContentAsync(searchUrl, propertyAddress, "/property/");
            }

            if (string.IsNullOrEmpty(pageContent))
            {
                Console.WriteLine($"Could not fetch property data for {propertyAddress} from Domain.com.au");
                return null;
            }

            return ParsePropertyData(pageContent, propertyAddress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from Domain.com.au: {ex.Message}");
            return null;
        }
    }

    private PropertyDataDto ParsePropertyData(string htmlContent, string address)
    {
        var propertyData = new PropertyDataDto();

        // Extract property type from HTML
        propertyData.PropertyType = ExtractPropertyType(htmlContent);
        propertyData.LandOwnership = "Freehold"; // Default assumption
        propertyData.HasClearTitle = true;
        propertyData.HasEncumbrances = false;
        propertyData.Zoning = DetermineZoning(address);
        propertyData.LocationCategory = DetermineLocationCategory(address);
        propertyData.DistanceToCbdKm = EstimateDistanceToCbd(address);

        // Extract school zone information
        propertyData.SchoolZoneQuality = ExtractSchoolZoneQuality(htmlContent);
        propertyData.DistanceToPublicTransportMeters = ExtractTransportDistance(htmlContent);

        // Extract rental and financial data
        propertyData.RentalYieldPercentage = ExtractRentalYield(htmlContent);
        propertyData.CapitalGrowthPercentage = ExtractCapitalGrowth(htmlContent);
        propertyData.VacancyRatePercentage = 2.0m; // Default based on national average
        propertyData.LocalDemand = DetermineLocalDemand(address);

        // Property condition
        propertyData.HasStructuralIssues = false;
        propertyData.PropertyAgeYears = ExtractPropertyAge(htmlContent);
        propertyData.HasMajorDefects = false;
        propertyData.MaintenanceLevel = "Minimal";
        propertyData.MeetsCurrentBuildingCodes = true;
        propertyData.HasRequiredCertificates = true;

        // Tenancy information
        propertyData.HasLongTermTenants = true;
        propertyData.HasReliablePaymentHistory = true;
        propertyData.LeaseRemainingMonths = 12;
        propertyData.HasConsistentRentalHistory = true;

        // Financial metrics
        propertyData.CashFlowCoverageRatio = 1.2m;
        propertyData.MeetsServiceabilityRequirements = true;
        propertyData.LoanToValueRatio = 75m;
        propertyData.AnnualInsuranceCost = 1500m;
        propertyData.SuitableForCrossCollateral = true;
        propertyData.EquityAvailable = 100000m;
        propertyData.EligibleForRefinance = true;

        // Market metrics
        propertyData.HasStableSaleHistory = true;
        propertyData.YearsSinceLastSale = ExtractYearsSinceLastSale(htmlContent);
        propertyData.DaysOnMarket = ExtractDaysOnMarket(htmlContent);
        propertyData.HasStrongComparables = true;
        propertyData.IsUniqueProperty = false;

        // Risk assessment
        propertyData.AcceptedByMajorLenders = true;
        propertyData.RiskRating = "Low";
        propertyData.HasDevelopmentRisk = false;
        propertyData.FitsPortfolioDiversity = true;
        propertyData.ViableForLongTermHold = true;

        return propertyData;
    }

    private string ExtractPropertyType(string htmlContent)
    {
        // Look for property type in HTML
        if (htmlContent.Contains("House", StringComparison.OrdinalIgnoreCase))
            return "House";
        if (htmlContent.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
            htmlContent.Contains("Unit", StringComparison.OrdinalIgnoreCase))
            return "Unit";
        if (htmlContent.Contains("Townhouse", StringComparison.OrdinalIgnoreCase))
            return "Townhouse";
        if (htmlContent.Contains("Duplex", StringComparison.OrdinalIgnoreCase))
            return "Duplex";

        return "House"; // Default
    }

    private string DetermineZoning(string address)
    {
        // Simple heuristic based on address patterns
        if (address.Contains("Industrial", StringComparison.OrdinalIgnoreCase))
            return "Industrial";
        if (address.Contains("Commercial", StringComparison.OrdinalIgnoreCase))
            return "Commercial";

        return "Residential";
    }

    private string DetermineLocationCategory(string address)
    {
        // Major Australian cities
        var metroCities = new[] { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Canberra" };

        foreach (var city in metroCities)
        {
            if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                return "Metro";
        }

        return "Regional";
    }

    private int EstimateDistanceToCbd(string address)
    {
        // TODO: Integrate with Google Maps API or similar for accurate distance
        // For now, use simple heuristics
        var innerSuburbs = new[] { "CBD", "City", "Central" };

        foreach (var suburb in innerSuburbs)
        {
            if (address.Contains(suburb, StringComparison.OrdinalIgnoreCase))
                return 5;
        }

        return 20; // Default estimate
    }

    private string ExtractSchoolZoneQuality(string htmlContent)
    {
        // Look for school mentions in the content
        if (htmlContent.Contains("selective", StringComparison.OrdinalIgnoreCase) ||
            htmlContent.Contains("top-rated", StringComparison.OrdinalIgnoreCase))
            return "Top-tier";

        if (htmlContent.Contains("good schools", StringComparison.OrdinalIgnoreCase))
            return "Good";

        return "Average";
    }

    private int ExtractTransportDistance(string htmlContent)
    {
        // Look for transport mentions
        if (htmlContent.Contains("train station nearby", StringComparison.OrdinalIgnoreCase))
            return 400;

        return 800; // Default estimate
    }

    private decimal ExtractRentalYield(string htmlContent)
    {
        // TODO: Parse actual rental yield from HTML or calculate from price/rent
        return 4.0m; // Default estimate
    }

    private decimal ExtractCapitalGrowth(string htmlContent)
    {
        // TODO: Parse historical growth data
        return 5.5m; // Default estimate
    }

    private int ExtractPropertyAge(string htmlContent)
    {
        // Look for build year or age mentions
        // TODO: Implement actual parsing
        return 15; // Default estimate
    }

    private int ExtractYearsSinceLastSale(string htmlContent)
    {
        // TODO: Parse last sale date
        return 3; // Default estimate
    }

    private int ExtractDaysOnMarket(string htmlContent)
    {
        // TODO: Parse actual days on market
        return 25; // Default estimate
    }

    private string DetermineLocalDemand(string address)
    {
        // Simple heuristic based on location
        var highDemandAreas = new[] { "Sydney", "Melbourne", "Inner" };

        foreach (var area in highDemandAreas)
        {
            if (address.Contains(area, StringComparison.OrdinalIgnoreCase))
                return "High";
        }

        return "Medium";
    }

    // Helper classes for JSON deserialization
    private class DomainSearchResponse
    {
        public List<DomainSearchResult>? Results { get; set; }
    }

    private class DomainSearchResult
    {
        public string? Id { get; set; }
        public string? Address { get; set; }
    }
}
