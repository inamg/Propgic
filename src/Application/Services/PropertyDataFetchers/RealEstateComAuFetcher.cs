using System.Text.Json;
using System.Text.RegularExpressions;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class RealEstateComAuFetcher : IPropertyDataFetcher
{
    private readonly ChatGptUrlDiscoveryService _chatGptService;
    private readonly SeleniumWebScraperService _seleniumService;
    private const string BaseUrl = "https://www.realestate.com.au";

    public int Priority => 2;
    public string SourceName => "RealEstate.com.au";

    public RealEstateComAuFetcher(ChatGptUrlDiscoveryService chatGptService, SeleniumWebScraperService seleniumService)
    {
        _chatGptService = chatGptService;
        _seleniumService = seleniumService;
    }

    public async Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress)
    {
        try
        {
            // Step 1: Use ChatGPT to get the suggested URL
            var realEstateUrl = await _chatGptService.GetPropertyUrlAsync(propertyAddress, "realestate.com.au");

            string? pageContent = null;

            if (!string.IsNullOrEmpty(realEstateUrl))
            {
                // Step 2: Use Selenium to fetch the page content from ChatGPT URL
                pageContent = await _seleniumService.GetPageContentAsync(realEstateUrl);
            }

            // Fallback: If ChatGPT URL failed, use Selenium to search directly
            if (string.IsNullOrEmpty(pageContent))
            {
                var searchUrl = $"{BaseUrl}/buy/";
                pageContent = await _seleniumService.SearchAndGetContentAsync(searchUrl, propertyAddress, "/property/");
            }

            if (string.IsNullOrEmpty(pageContent))
            {
                Console.WriteLine($"Could not fetch property data for {propertyAddress} from RealEstate.com.au");
                return null;
            }

            return ParsePropertyData(pageContent, propertyAddress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from RealEstate.com.au: {ex.Message}");
            return null;
        }
    }

    public async Task<PropertyDataDto?> FetchPropertyDataFromUrlAsync(string propertyUrl)
    {
        try
        {
            Console.WriteLine($"Fetching property data directly from URL: {propertyUrl}");

            // Use Selenium to fetch the page content from the provided URL
            var pageContent = await _seleniumService.GetPageContentAsync(propertyUrl);

            if (string.IsNullOrEmpty(pageContent))
            {
                Console.WriteLine($"Could not fetch property data from URL: {propertyUrl}");
                return null;
            }

            return ParsePropertyData(pageContent, propertyUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from URL {propertyUrl}: {ex.Message}");
            return null;
        }
    }

    private PropertyDataDto ParsePropertyData(string htmlContent, string address)
    {
        var propertyData = new PropertyDataDto
        {
            PropertyType = ExtractPropertyType(htmlContent),
            LandOwnership = ExtractLandOwnership(htmlContent),
            HasClearTitle = true,
            HasEncumbrances = false,
            Zoning = "Residential",
            LocationCategory = DetermineLocationCategory(address),
            DistanceToCbdKm = EstimateDistanceToCbd(address),
            SchoolZoneQuality = ExtractSchoolZoneQuality(htmlContent),
            DistanceToPublicTransportMeters = 600,
            RentalYieldPercentage = CalculateRentalYield(htmlContent),
            CapitalGrowthPercentage = ExtractCapitalGrowth(htmlContent),
            VacancyRatePercentage = ExtractVacancyRate(htmlContent),
            LocalDemand = DetermineLocalDemand(address),
            HasStructuralIssues = false,
            PropertyAgeYears = ExtractPropertyAge(htmlContent),
            HasMajorDefects = false,
            MaintenanceLevel = DetermineMaintenanceLevel(htmlContent),
            MeetsCurrentBuildingCodes = true,
            HasRequiredCertificates = true,
            HasLongTermTenants = true,
            HasReliablePaymentHistory = true,
            LeaseRemainingMonths = 12,
            HasConsistentRentalHistory = true,
            CashFlowCoverageRatio = CalculateCashFlowCoverage(htmlContent),
            MeetsServiceabilityRequirements = true,
            LoanToValueRatio = 70m,
            AnnualInsuranceCost = EstimateInsuranceCost(htmlContent),
            SuitableForCrossCollateral = true,
            EquityAvailable = EstimateEquity(htmlContent),
            EligibleForRefinance = true,
            HasStableSaleHistory = true,
            YearsSinceLastSale = ExtractYearsSinceLastSale(htmlContent),
            DaysOnMarket = ExtractDaysOnMarket(htmlContent),
            HasStrongComparables = true,
            IsUniqueProperty = IsUniqueProperty(htmlContent),
            AcceptedByMajorLenders = true,
            RiskRating = AssessRisk(htmlContent, address),
            HasDevelopmentRisk = CheckDevelopmentRisk(address),
            FitsPortfolioDiversity = true,
            ViableForLongTermHold = true
        };

        return propertyData;
    }

    private string ExtractPropertyType(string htmlContent)
    {
        var propertyTypePatterns = new Dictionary<string, string>
        {
            { @"House", "House" },
            { @"Apartment|Unit", "Unit" },
            { @"Townhouse", "Townhouse" },
            { @"Duplex", "Duplex" },
            { @"Villa", "Townhouse" }
        };

        foreach (var pattern in propertyTypePatterns)
        {
            if (Regex.IsMatch(htmlContent, pattern.Key, RegexOptions.IgnoreCase))
                return pattern.Value;
        }

        return "House";
    }

    private string ExtractLandOwnership(string htmlContent)
    {
        if (Regex.IsMatch(htmlContent, @"strata|body corporate", RegexOptions.IgnoreCase))
            return "Strata";

        if (Regex.IsMatch(htmlContent, @"leasehold", RegexOptions.IgnoreCase))
            return "Leasehold";

        return "Freehold";
    }

    private string DetermineLocationCategory(string address)
    {
        var metroCities = new[] { "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast", "Newcastle", "Canberra", "Sunshine Coast", "Wollongong" };

        foreach (var city in metroCities)
        {
            if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                return "Metro";
        }

        return "Regional";
    }

    private int EstimateDistanceToCbd(string address)
    {
        // Inner suburbs typically within 10km
        var innerSuburbs = new[] { "CBD", "City", "Central", "Inner", "Surry Hills", "Fitzroy", "Fortitude Valley" };

        foreach (var suburb in innerSuburbs)
        {
            if (address.Contains(suburb, StringComparison.OrdinalIgnoreCase))
                return 5;
        }

        return 15;
    }

    private string ExtractSchoolZoneQuality(string htmlContent)
    {
        if (Regex.IsMatch(htmlContent, @"selective|prestigious|top rated|high-performing", RegexOptions.IgnoreCase))
            return "Top-tier";

        if (Regex.IsMatch(htmlContent, @"good schools|quality education", RegexOptions.IgnoreCase))
            return "Good";

        return "Average";
    }

    private decimal CalculateRentalYield(string htmlContent)
    {
        try
        {
            // Try to extract price and rental estimate
            var priceMatch = Regex.Match(htmlContent, @"\$([0-9,]+)(?:,000)?");
            var rentMatch = Regex.Match(htmlContent, @"\$([0-9,]+)\s*(?:per|\/)\s*week", RegexOptions.IgnoreCase);

            if (priceMatch.Success && rentMatch.Success)
            {
                var price = decimal.Parse(priceMatch.Groups[1].Value.Replace(",", ""));
                var weeklyRent = decimal.Parse(rentMatch.Groups[1].Value.Replace(",", ""));
                var annualRent = weeklyRent * 52;

                if (price > 0)
                {
                    var yield = (annualRent / price) * 100;
                    return Math.Round(yield, 2);
                }
            }
        }
        catch { }

        return 4.2m; // Default estimate
    }

    private decimal ExtractCapitalGrowth(string htmlContent)
    {
        // Look for growth mentions
        var growthMatch = Regex.Match(htmlContent, @"([0-9.]+)%\s*(?:growth|increase)", RegexOptions.IgnoreCase);

        if (growthMatch.Success && decimal.TryParse(growthMatch.Groups[1].Value, out var growth))
        {
            return growth;
        }

        return 5.8m; // Default estimate based on Australian average
    }

    private decimal ExtractVacancyRate(string htmlContent)
    {
        // TODO: Integrate with vacancy rate data sources
        return 2.5m; // Australian national average
    }

    private string DetermineLocalDemand(string address)
    {
        var highDemandKeywords = new[] { "Sydney", "Melbourne", "Inner", "Eastern Suburbs", "North Shore" };

        foreach (var keyword in highDemandKeywords)
        {
            if (address.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return "High";
        }

        return "Medium";
    }

    private int ExtractPropertyAge(string htmlContent)
    {
        var yearMatch = Regex.Match(htmlContent, @"(?:built|constructed)\s*(?:in\s*)?([0-9]{4})", RegexOptions.IgnoreCase);

        if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var builtYear))
        {
            return DateTime.Now.Year - builtYear;
        }

        return 20; // Default estimate
    }

    private string DetermineMaintenanceLevel(string htmlContent)
    {
        if (Regex.IsMatch(htmlContent, @"renovated|updated|modern|new", RegexOptions.IgnoreCase))
            return "Minimal";

        if (Regex.IsMatch(htmlContent, @"needs work|fixer|original condition", RegexOptions.IgnoreCase))
            return "Extensive";

        return "Moderate";
    }

    private decimal CalculateCashFlowCoverage(string htmlContent)
    {
        // Estimate based on rental yield
        var rentalYield = CalculateRentalYield(htmlContent);

        if (rentalYield >= 5.0m)
            return 1.4m;
        if (rentalYield >= 4.0m)
            return 1.2m;

        return 1.1m;
    }

    private decimal EstimateInsuranceCost(string htmlContent)
    {
        var propertyType = ExtractPropertyType(htmlContent);

        return propertyType.ToLower() switch
        {
            "unit" or "apartment" => 800m,
            "house" => 1500m,
            "townhouse" => 1200m,
            _ => 1300m
        };
    }

    private decimal EstimateEquity(string htmlContent)
    {
        // Try to extract property price
        var priceMatch = Regex.Match(htmlContent, @"\$([0-9,]+)(?:,000)?");

        if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value.Replace(",", ""), out var price))
        {
            // Assume 30% equity
            return price * 0.30m;
        }

        return 120000m; // Default estimate
    }

    private int ExtractYearsSinceLastSale(string htmlContent)
    {
        var saleMatch = Regex.Match(htmlContent, @"(?:sold|last sale)\s*(?:in\s*)?([0-9]{4})", RegexOptions.IgnoreCase);

        if (saleMatch.Success && int.TryParse(saleMatch.Groups[1].Value, out var saleYear))
        {
            return DateTime.Now.Year - saleYear;
        }

        return 4; // Default estimate
    }

    private int ExtractDaysOnMarket(string htmlContent)
    {
        var daysMatch = Regex.Match(htmlContent, @"([0-9]+)\s*days?\s*(?:on market|listed)", RegexOptions.IgnoreCase);

        if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out var days))
        {
            return days;
        }

        return 28; // Default estimate
    }

    private bool IsUniqueProperty(string htmlContent)
    {
        var uniqueKeywords = new[] { "heritage", "landmark", "character", "unique", "one-of-a-kind", "architect designed" };

        foreach (var keyword in uniqueKeywords)
        {
            if (htmlContent.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private string AssessRisk(string htmlContent, string address)
    {
        var riskFactors = 0;

        // Check for high-risk indicators
        if (Regex.IsMatch(htmlContent, @"flood|fire zone|unstable", RegexOptions.IgnoreCase))
            riskFactors += 2;

        if (ExtractPropertyAge(htmlContent) > 50)
            riskFactors += 1;

        if (address.Contains("Rural", StringComparison.OrdinalIgnoreCase))
            riskFactors += 1;

        return riskFactors switch
        {
            0 => "Low",
            1 or 2 => "Medium",
            _ => "High"
        };
    }

    private bool CheckDevelopmentRisk(string address)
    {
        // Areas with high development activity
        var developmentAreas = new[] { "Parramatta", "Olympic Park", "Green Square", "Fishermans Bend" };

        foreach (var area in developmentAreas)
        {
            if (address.Contains(area, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    // Helper classes for JSON deserialization
    private class RealEstateSearchResponse
    {
        public List<RealEstateSuggestion>? Suggestions { get; set; }
    }

    private class RealEstateSuggestion
    {
        public string? Id { get; set; }
        public string? Url { get; set; }
        public string? DisplayText { get; set; }
    }
}
