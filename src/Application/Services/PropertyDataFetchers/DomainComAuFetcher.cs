using System.Text.RegularExpressions;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class DomainComAuFetcher : IPropertyDataFetcher
{
    private readonly ChatGptUrlDiscoveryService _chatGptService;

    public int Priority => 1;
    public string SourceName => "Domain.com.au";

    public DomainComAuFetcher(ChatGptUrlDiscoveryService chatGptService)
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
            Console.WriteLine($"Error fetching from Domain.com.au: {ex.Message}");
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
            // Domain.com.au URLs typically look like: https://www.domain.com.au/123-example-street-suburb-state-1234567
            var uri = new Uri(url);
            var path = uri.AbsolutePath.Trim('/');

            // Remove common prefixes
            if (path.StartsWith("property-profile/", StringComparison.OrdinalIgnoreCase))
                path = path.Substring("property-profile/".Length);

            // Convert URL slug to address format
            var parts = path.Split('-');
            if (parts.Length < 3) return null;

            // Try to reconstruct address from URL slug
            var address = string.Join(" ", parts).Replace("  ", " ");
            return address;
        }
        catch
        {
            return null;
        }
    }

    private PropertyDataDto ParsePropertyData(string htmlContent, string address)
    {
        var propertyData = new PropertyDataDto();

        // Extract basic property information from Domain.com.au HTML
        propertyData.PropertyType = ExtractPropertyType(htmlContent);
        propertyData.LandOwnership = ExtractLandOwnership(htmlContent);
        propertyData.HasClearTitle = true;
        propertyData.HasEncumbrances = false;
        propertyData.Zoning = DetermineZoning(htmlContent, address);
        propertyData.LocationCategory = DetermineLocationCategory(htmlContent, address);
        propertyData.DistanceToCbdKm = EstimateDistanceToCbd(htmlContent, address);

        // Extract school zone and transport information
        propertyData.SchoolZoneQuality = ExtractSchoolZoneQuality(htmlContent);
        propertyData.DistanceToPublicTransportMeters = ExtractTransportDistance(htmlContent);

        // Extract price and calculate rental/financial data
        var price = ExtractPrice(htmlContent);
        var bedrooms = ExtractBedrooms(htmlContent);
        var landSize = ExtractLandSize(htmlContent);

        propertyData.RentalYieldPercentage = CalculateRentalYield(price, bedrooms, propertyData.LocationCategory);
        propertyData.CapitalGrowthPercentage = ExtractCapitalGrowth(htmlContent, propertyData.LocationCategory);
        propertyData.VacancyRatePercentage = DetermineVacancyRate(propertyData.LocationCategory);
        propertyData.LocalDemand = DetermineLocalDemand(htmlContent, address);

        // Property condition based on age and features
        propertyData.PropertyAgeYears = ExtractPropertyAge(htmlContent);
        propertyData.HasStructuralIssues = DetermineStructuralIssues(htmlContent, propertyData.PropertyAgeYears);
        propertyData.HasMajorDefects = DetermineMajorDefects(htmlContent);
        propertyData.MaintenanceLevel = DetermineMaintenanceLevel(htmlContent, propertyData.PropertyAgeYears);
        propertyData.MeetsCurrentBuildingCodes = !propertyData.PropertyAgeYears.HasValue || propertyData.PropertyAgeYears.Value < 30;
        propertyData.HasRequiredCertificates = true;

        // Tenancy information
        propertyData.HasLongTermTenants = ExtractTenancyStatus(htmlContent);
        propertyData.HasReliablePaymentHistory = true;
        propertyData.LeaseRemainingMonths = ExtractLeaseRemaining(htmlContent);
        propertyData.HasConsistentRentalHistory = true;

        // Financial metrics based on price
        propertyData.CashFlowCoverageRatio = CalculateCashFlowCoverage(price, propertyData.RentalYieldPercentage);
        propertyData.MeetsServiceabilityRequirements = price < 2000000;
        propertyData.LoanToValueRatio = 80m;
        propertyData.AnnualInsuranceCost = CalculateInsuranceCost(price, propertyData.PropertyType);
        propertyData.SuitableForCrossCollateral = price > 500000 && propertyData.PropertyType != "Unit";
        propertyData.EquityAvailable = price * 0.2m;
        propertyData.EligibleForRefinance = !propertyData.PropertyAgeYears.HasValue || propertyData.PropertyAgeYears.Value < 40;

        // Market metrics
        propertyData.HasStableSaleHistory = ExtractSaleHistory(htmlContent);
        propertyData.YearsSinceLastSale = ExtractYearsSinceLastSale(htmlContent);
        propertyData.DaysOnMarket = ExtractDaysOnMarket(htmlContent);
        propertyData.HasStrongComparables = true;
        propertyData.IsUniqueProperty = DetermineUniqueness(landSize, propertyData.PropertyType);

        // Risk assessment
        propertyData.AcceptedByMajorLenders = DetermineLenderAcceptance(propertyData.PropertyType, landSize, price);
        propertyData.RiskRating = DetermineRiskRating(propertyData);
        propertyData.HasDevelopmentRisk = ExtractDevelopmentRisk(htmlContent);
        propertyData.FitsPortfolioDiversity = true;
        propertyData.ViableForLongTermHold = propertyData.LocationCategory == "Metro" || (propertyData.CapitalGrowthPercentage.HasValue && propertyData.CapitalGrowthPercentage.Value > 3);

        Console.WriteLine($"Parsed property: Type={propertyData.PropertyType}, Price=${price:N0}, Beds={bedrooms}, Land={landSize}sqm, Age={propertyData.PropertyAgeYears}yrs");

        return propertyData;
    }

    #region Property Type and Basic Info Extraction

    private string ExtractPropertyType(string htmlContent)
    {
        // Domain.com.au uses property type in various places - meta tags, breadcrumbs, and listing details
        // Pattern: "property-info-address" or "listing-details__summary-title" sections

        var patterns = new[]
        {
            @"property[_-]?type[""']?\s*[>:]\s*([^<,]+)",
            @"<span[^>]*class=""[^""]*property-feature[^""]*""[^>]*>\s*(\w+)\s*</span>",
            @"listing-details__summary-title[^>]*>\s*(\w+)\s+for",
            @"breadcrumb[^>]*>\s*<[^>]*>\s*(House|Apartment|Unit|Townhouse|Villa|Duplex|Land)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(htmlContent, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var type = match.Groups[1].Value.Trim();
                return NormalizePropertyType(type);
            }
        }

        // Fallback: search for property type keywords in content
        if (Regex.IsMatch(htmlContent, @"\b(apartment|unit|flat)\b", RegexOptions.IgnoreCase))
            return "Unit";
        if (Regex.IsMatch(htmlContent, @"\btownhouse\b", RegexOptions.IgnoreCase))
            return "Townhouse";
        if (Regex.IsMatch(htmlContent, @"\bvilla\b", RegexOptions.IgnoreCase))
            return "Villa";
        if (Regex.IsMatch(htmlContent, @"\bduplex\b", RegexOptions.IgnoreCase))
            return "Duplex";
        if (Regex.IsMatch(htmlContent, @"\bvacant\s+land\b", RegexOptions.IgnoreCase))
            return "Land";

        return "House";
    }

    private string NormalizePropertyType(string type)
    {
        type = type.Trim().ToLower();
        return type switch
        {
            "apartment" or "unit" or "flat" => "Unit",
            "townhouse" or "terrace" => "Townhouse",
            "villa" => "Villa",
            "duplex" or "semi" => "Duplex",
            "land" or "vacant land" => "Land",
            _ => "House"
        };
    }

    private string ExtractLandOwnership(string htmlContent)
    {
        // Look for strata/body corporate mentions
        if (Regex.IsMatch(htmlContent, @"\b(strata|body\s+corporate|owners\s+corporation)\b", RegexOptions.IgnoreCase))
            return "Strata";
        if (Regex.IsMatch(htmlContent, @"\bleasehold\b", RegexOptions.IgnoreCase))
            return "Leasehold";

        return "Freehold";
    }

    #endregion

    #region Price and Property Features Extraction

    private decimal ExtractPrice(string htmlContent)
    {
        // Domain.com.au price patterns
        var patterns = new[]
        {
            @"\$\s*([\d,]+(?:\.\d{2})?)\s*(?:million|m)\b",
            @"price[""']?\s*[>:]\s*\$?\s*([\d,]+)",
            @"listing-details__summary-price[^>]*>\s*\$?\s*([\d,]+)",
            @"\$\s*([\d]{1,3}(?:,\d{3})*(?:\.\d{2})?)",
            @"([\d]{1,3}(?:,\d{3})+)\s*(?:asking|guide|price)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(htmlContent, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var priceStr = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(priceStr, out var price))
                {
                    // Check if it's in millions
                    if (htmlContent.ToLower().Contains("million") || htmlContent.ToLower().Contains(" m "))
                    {
                        if (price < 100) price *= 1_000_000;
                    }
                    if (price > 10000) return price;
                }
            }
        }

        return 800000m; // Default estimate
    }

    private int ExtractBedrooms(string htmlContent)
    {
        // Domain uses icons with bed count - patterns like "4 Beds" or data attributes
        var patterns = new[]
        {
            @"(\d+)\s*(?:bed|bedroom|br)\b",
            @"bed[^>]*>\s*(\d+)",
            @"(\d+)\s*<[^>]*bed",
            @"property-feature__feature[^>]*bed[^>]*>\s*(\d+)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(htmlContent, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var beds))
            {
                if (beds is >= 1 and <= 10) return beds;
            }
        }

        return 3; // Default
    }

    private int ExtractLandSize(string htmlContent)
    {
        // Land size patterns - "450 m²", "450m2", "450 sqm", "land size: 450"
        var patterns = new[]
        {
            @"land\s*(?:size|area)?[:\s]*(\d+(?:,\d+)?)\s*(?:m²|m2|sqm|square)",
            @"(\d{2,4})\s*(?:m²|m2|sqm)\s*(?:land|block|lot)",
            @"(\d{2,4})\s*(?:m²|m2|sqm)",
            @"block\s*(?:size)?[:\s]*(\d+)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(htmlContent, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var sizeStr = match.Groups[1].Value.Replace(",", "");
                if (int.TryParse(sizeStr, out var size) && size is >= 50 and <= 100000)
                {
                    return size;
                }
            }
        }

        return 600; // Default suburban block
    }

    #endregion

    #region Location and Zoning

    private string DetermineZoning(string htmlContent, string address)
    {
        if (Regex.IsMatch(htmlContent, @"\b(commercial|retail|shop|office)\s*(zone|property|use)\b", RegexOptions.IgnoreCase))
            return "Commercial";
        if (Regex.IsMatch(htmlContent, @"\b(industrial|warehouse|factory)\b", RegexOptions.IgnoreCase))
            return "Industrial";
        if (Regex.IsMatch(htmlContent, @"\b(mixed\s*use|live.work)\b", RegexOptions.IgnoreCase))
            return "Mixed";
        if (Regex.IsMatch(htmlContent, @"\b(rural|farm|acreage|agricultural)\b", RegexOptions.IgnoreCase))
            return "Rural";

        return "Residential";
    }

    private string DetermineLocationCategory(string htmlContent, string address)
    {
        var combined = $"{htmlContent} {address}".ToLower();

        // NSW suburbs classification
        var metroSuburbs = new[]
        {
            "sydney", "parramatta", "chatswood", "bondi", "manly", "newcastle",
            "wollongong", "kellyville", "castle hill", "rouse hill", "baulkham hills",
            "north kellyville", "bella vista", "norwest"
        };

        var regionalCities = new[]
        {
            "dubbo", "tamworth", "wagga", "orange", "bathurst", "albury", "port macquarie"
        };

        foreach (var suburb in metroSuburbs)
        {
            if (combined.Contains(suburb))
                return "Metro";
        }

        foreach (var city in regionalCities)
        {
            if (combined.Contains(city))
                return "Regional";
        }

        // Check state capitals
        if (Regex.IsMatch(combined, @"\b(melbourne|brisbane|perth|adelaide|canberra|hobart|darwin)\b"))
            return "Metro";

        // Check for NSW postcode patterns (2000-2999 metro, others regional)
        var postcodeMatch = Regex.Match(address, @"\b(2\d{3})\b");
        if (postcodeMatch.Success && int.TryParse(postcodeMatch.Groups[1].Value, out var postcode))
        {
            if (postcode is >= 2000 and <= 2234 or >= 2555 and <= 2574 or >= 2745 and <= 2770)
                return "Metro";
        }

        return "Regional";
    }

    private int EstimateDistanceToCbd(string htmlContent, string address)
    {
        var combined = $"{htmlContent} {address}".ToLower();

        // Check for distance mentions
        var distMatch = Regex.Match(combined, @"(\d+(?:\.\d+)?)\s*km\s*(?:to|from)\s*(?:cbd|city)", RegexOptions.IgnoreCase);
        if (distMatch.Success && int.TryParse(distMatch.Groups[1].Value, out var dist))
            return dist;

        // Estimate based on suburb knowledge
        var innerSuburbs = new Dictionary<string, int>
        {
            { "cbd", 0 }, { "city", 0 }, { "central", 2 },
            { "surry hills", 2 }, { "pyrmont", 3 }, { "glebe", 4 },
            { "newtown", 5 }, { "bondi", 8 }, { "manly", 15 },
            { "parramatta", 24 }, { "chatswood", 10 }, { "hornsby", 25 },
            { "kellyville", 35 }, { "north kellyville", 38 }, { "castle hill", 30 },
            { "rouse hill", 40 }, { "bella vista", 32 }
        };

        foreach (var (suburb, distance) in innerSuburbs)
        {
            if (combined.Contains(suburb))
                return distance;
        }

        return 25; // Default
    }

    #endregion

    #region Schools and Transport

    private string ExtractSchoolZoneQuality(string htmlContent)
    {
        var content = htmlContent.ToLower();

        // Look for school names and quality indicators
        if (Regex.IsMatch(content, @"\b(selective|grammar|private|prestigious)\s*school\b"))
            return "Top-tier";

        if (Regex.IsMatch(content, @"\b(excellent|great|quality)\s*school\b"))
            return "Good";

        // Check for specific school catchment mentions
        if (Regex.IsMatch(content, @"\bschool\s*(catchment|zone)\b"))
            return "Good";

        if (content.Contains("school"))
            return "Average";

        return "Average";
    }

    private int ExtractTransportDistance(string htmlContent)
    {
        var content = htmlContent.ToLower();

        // Look for specific distance mentions
        var distMatch = Regex.Match(content, @"(\d+)\s*(?:m|meters?|metres?)\s*(?:to|from)\s*(?:station|bus|train|transport)");
        if (distMatch.Success && int.TryParse(distMatch.Groups[1].Value, out var meters))
            return meters;

        // Keyword-based estimation
        if (Regex.IsMatch(content, @"\b(next\s+to|adjacent|opposite)\s*(?:station|bus\s+stop)\b"))
            return 100;
        if (Regex.IsMatch(content, @"\b(walking\s+distance|short\s+walk)\s*(?:to)?\s*(?:station|transport)\b"))
            return 400;
        if (content.Contains("train station") || content.Contains("metro station"))
            return 600;
        if (content.Contains("bus"))
            return 300;

        return 800;
    }

    #endregion

    #region Financial Calculations

    private static decimal? CalculateRentalYield(decimal price, int bedrooms, string? locationCategory)
    {
        if (price <= 0) return 4.0m;

        // Estimate weekly rent based on bedrooms and location
        decimal weeklyRent = (bedrooms, locationCategory) switch
        {
            (1, "Metro") => 500m,
            (2, "Metro") => 650m,
            (3, "Metro") => 750m,
            (4, "Metro") => 900m,
            (>= 5, "Metro") => 1100m,
            (1, _) => 350m,
            (2, _) => 450m,
            (3, _) => 550m,
            (4, _) => 650m,
            _ => 750m
        };

        var annualRent = weeklyRent * 52;
        var yield = (annualRent / price) * 100;

        return Math.Round(yield, 2);
    }

    private static decimal? ExtractCapitalGrowth(string htmlContent, string? locationCategory)
    {
        // Look for growth mentions in content
        var growthMatch = Regex.Match(htmlContent, @"(\d+(?:\.\d+)?)\s*%?\s*(?:growth|appreciation|increase)", RegexOptions.IgnoreCase);
        if (growthMatch.Success && decimal.TryParse(growthMatch.Groups[1].Value, out var growth))
        {
            if (growth is > 0 and < 30) return growth;
        }

        // Default based on location
        return locationCategory switch
        {
            "Metro" => 6.5m,
            "Regional" => 4.5m,
            _ => 5.0m
        };
    }

    private static decimal? DetermineVacancyRate(string? locationCategory)
    {
        return locationCategory switch
        {
            "Metro" => 1.5m,
            "Regional" => 2.5m,
            _ => 2.0m
        };
    }

    private static decimal? CalculateCashFlowCoverage(decimal price, decimal? rentalYield)
    {
        if (price <= 0 || !rentalYield.HasValue || rentalYield.Value <= 0) return 1.0m;

        var annualRent = price * (rentalYield.Value / 100);
        var estimatedMortgage = price * 0.8m * 0.06m; // 80% LVR at 6% interest

        return annualRent > 0 ? Math.Round(annualRent / estimatedMortgage, 2) : 1.0m;
    }

    private static decimal? CalculateInsuranceCost(decimal price, string? propertyType)
    {
        var baseRate = propertyType switch
        {
            "Unit" => 0.001m,
            "Townhouse" => 0.0012m,
            _ => 0.0015m
        };

        return Math.Round(price * baseRate, 0);
    }

    #endregion

    #region Property Condition Assessment

    private int ExtractPropertyAge(string htmlContent)
    {
        // Look for build year
        var yearMatch = Regex.Match(htmlContent, @"\b(built|constructed|erected)\s*(?:in)?\s*(19\d{2}|20[0-2]\d)\b", RegexOptions.IgnoreCase);
        if (yearMatch.Success && int.TryParse(yearMatch.Groups[2].Value, out var year))
        {
            return DateTime.Now.Year - year;
        }

        // Look for age mentions
        var ageMatch = Regex.Match(htmlContent, @"(\d+)\s*(?:year|yr)s?\s*old\b", RegexOptions.IgnoreCase);
        if (ageMatch.Success && int.TryParse(ageMatch.Groups[1].Value, out var age))
        {
            return age;
        }

        // Keyword-based estimation
        if (Regex.IsMatch(htmlContent, @"\b(brand\s*new|newly\s*built|just\s*completed)\b", RegexOptions.IgnoreCase))
            return 1;
        if (Regex.IsMatch(htmlContent, @"\b(modern|contemporary|recent)\b", RegexOptions.IgnoreCase))
            return 5;
        if (Regex.IsMatch(htmlContent, @"\b(renovated|updated|refreshed)\b", RegexOptions.IgnoreCase))
            return 15;
        if (Regex.IsMatch(htmlContent, @"\b(character|period|heritage|federation)\b", RegexOptions.IgnoreCase))
            return 80;
        if (Regex.IsMatch(htmlContent, @"\b(original|classic)\b", RegexOptions.IgnoreCase))
            return 40;

        return 20;
    }

    private static bool DetermineStructuralIssues(string htmlContent, int? propertyAge)
    {
        if (Regex.IsMatch(htmlContent, @"\b(structural\s*issues?|foundation\s*problems?|subsidence|cracking)\b", RegexOptions.IgnoreCase))
            return true;

        return propertyAge.HasValue && propertyAge.Value > 60;
    }

    private bool DetermineMajorDefects(string htmlContent)
    {
        return Regex.IsMatch(htmlContent, @"\b(major\s*defects?|significant\s*repairs?|asbestos|termite\s*damage)\b", RegexOptions.IgnoreCase);
    }

    private static string DetermineMaintenanceLevel(string htmlContent, int? propertyAge)
    {
        if (Regex.IsMatch(htmlContent, @"\b(immaculate|pristine|perfect\s*condition|no\s*work\s*required)\b", RegexOptions.IgnoreCase))
            return "Minimal";
        if (Regex.IsMatch(htmlContent, @"\b(good\s*condition|well\s*maintained|neat)\b", RegexOptions.IgnoreCase))
            return "Minimal";
        if (Regex.IsMatch(htmlContent, @"\b(needs?\s*work|renovation\s*potential|handyman|fixer)\b", RegexOptions.IgnoreCase))
            return "Extensive";
        if (Regex.IsMatch(htmlContent, @"\b(some\s*work|minor\s*repairs?|could\s*use)\b", RegexOptions.IgnoreCase))
            return "Moderate";

        return propertyAge.HasValue && propertyAge.Value > 30 ? "Moderate" : "Minimal";
    }

    #endregion

    #region Tenancy and Lease

    private bool ExtractTenancyStatus(string htmlContent)
    {
        if (Regex.IsMatch(htmlContent, @"\b(tenanted|leased|rental\s*income|currently\s*rented)\b", RegexOptions.IgnoreCase))
            return true;
        if (Regex.IsMatch(htmlContent, @"\b(vacant\s*possession|owner\s*occupied|move\s*straight\s*in)\b", RegexOptions.IgnoreCase))
            return false;

        return false;
    }

    private int ExtractLeaseRemaining(string htmlContent)
    {
        var leaseMatch = Regex.Match(htmlContent, @"(\d+)\s*(?:month|mth)s?\s*(?:remaining|left|lease)", RegexOptions.IgnoreCase);
        if (leaseMatch.Success && int.TryParse(leaseMatch.Groups[1].Value, out var months))
            return months;

        if (ExtractTenancyStatus(htmlContent))
            return 6; // Assume some lease remaining if tenanted

        return 0;
    }

    #endregion

    #region Market and Sale History

    private bool ExtractSaleHistory(string htmlContent)
    {
        // Check for sale history section
        return Regex.IsMatch(htmlContent, @"\b(sale\s*history|previous(ly)?\s*sold|last\s*sold)\b", RegexOptions.IgnoreCase);
    }

    private int ExtractYearsSinceLastSale(string htmlContent)
    {
        // Look for last sold date
        var soldMatch = Regex.Match(htmlContent, @"(?:last\s*)?sold\s*(?:in|on)?\s*(?:\w+\s*)?(20[0-2]\d|19\d{2})", RegexOptions.IgnoreCase);
        if (soldMatch.Success && int.TryParse(soldMatch.Groups[1].Value, out var year))
        {
            return DateTime.Now.Year - year;
        }

        var monthsMatch = Regex.Match(htmlContent, @"sold\s*(\d+)\s*(?:month|year)s?\s*ago", RegexOptions.IgnoreCase);
        if (monthsMatch.Success && int.TryParse(monthsMatch.Groups[1].Value, out var time))
        {
            if (htmlContent.ToLower().Contains("month"))
                return time / 12;
            return time;
        }

        return 5; // Default
    }

    private int ExtractDaysOnMarket(string htmlContent)
    {
        var daysMatch = Regex.Match(htmlContent, @"(\d+)\s*days?\s*(?:on\s*market|listed)", RegexOptions.IgnoreCase);
        if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out var days))
            return days;

        if (Regex.IsMatch(htmlContent, @"\b(just\s*listed|new\s*listing|hot\s*property)\b", RegexOptions.IgnoreCase))
            return 7;

        return 30;
    }

    private string DetermineLocalDemand(string htmlContent, string address)
    {
        var combined = $"{htmlContent} {address}".ToLower();

        if (Regex.IsMatch(combined, @"\b(high\s*demand|sought\s*after|popular|hot\s*market)\b"))
            return "High";
        if (Regex.IsMatch(combined, @"\b(low\s*demand|quiet\s*market|buyers?\s*market)\b"))
            return "Low";

        // High demand Sydney areas
        var highDemandAreas = new[] { "sydney", "inner west", "eastern suburbs", "north shore", "northern beaches" };
        foreach (var area in highDemandAreas)
        {
            if (combined.Contains(area))
                return "High";
        }

        return "Medium";
    }

    #endregion

    #region Risk Assessment

    private static bool DetermineUniqueness(int landSize, string? propertyType)
    {
        // Large blocks or unusual property types are considered unique
        if (landSize > 2000) return true;
        if (propertyType is "Land" or "Duplex") return true;

        return false;
    }

    private static bool DetermineLenderAcceptance(string? propertyType, int landSize, decimal price)
    {
        // Lenders may reject certain properties
        if (propertyType == "Land") return false;
        if (landSize < 40) return false; // Tiny lots
        if (price < 100000) return false; // Too cheap

        return true;
    }

    private static string DetermineRiskRating(PropertyDataDto data)
    {
        var riskScore = 0;

        if (data.HasStructuralIssues == true) riskScore += 3;
        if (data.HasMajorDefects == true) riskScore += 3;
        if (data.PropertyAgeYears.HasValue && data.PropertyAgeYears.Value > 50) riskScore += 2;
        if (data.MaintenanceLevel == "Extensive") riskScore += 2;
        if (data.VacancyRatePercentage.HasValue && data.VacancyRatePercentage.Value > 5) riskScore += 2;
        if (data.LocationCategory != "Metro") riskScore += 1;
        if (data.AcceptedByMajorLenders == false) riskScore += 3;

        return riskScore switch
        {
            <= 2 => "Low",
            <= 5 => "Medium",
            _ => "High"
        };
    }

    private bool ExtractDevelopmentRisk(string htmlContent)
    {
        return Regex.IsMatch(htmlContent, @"\b(development\s*site|subdivision\s*potential|da\s*approved|rezoning)\b", RegexOptions.IgnoreCase);
    }

    #endregion
}
