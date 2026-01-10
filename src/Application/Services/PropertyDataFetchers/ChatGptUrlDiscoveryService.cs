using System.Text.Json;
using OpenAI.Chat;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class ChatGptUrlDiscoveryService
{
    private readonly OpenAI.OpenAIClient _openAIClient;

    public ChatGptUrlDiscoveryService(string apiKey)
    {
        _openAIClient = new OpenAI.OpenAIClient(apiKey);
    }

    public async Task<string?> GetPropertyUrlAsync(string propertyAddress, string websiteName)
    {
        try
        {
            var prompt = $@"Given the property address: '{propertyAddress}'

Please construct or suggest the most likely property listing URL on {websiteName}.

Return ONLY the URL, nothing else. If you cannot determine a specific URL, return an empty string.";

            var chatClient = _openAIClient.GetChatClient("gpt-5.2");
            var result = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("You are a helpful assistant that constructs property listing URLs for Australian real estate websites."),
                new UserChatMessage(prompt)
            ]);

            var url = result.Value.Content[0].Text?.Trim();

            // Validate that it's a proper URL
            if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return url;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting URL from ChatGPT: {ex.Message}");
            return null;
        }
    }

    public async Task<PropertyDataDto?> GetPropertyDataAsync(string propertyAddress)
    {
        try
        {
            Console.WriteLine($"Asking OpenAI for property data: {propertyAddress}");

            var prompt = $@"You are an expert Australian property analyst with access to current property market data.

For the property at: {propertyAddress}

Please provide detailed property analysis data based on your knowledge of:
- The suburb and local area characteristics
- Typical property values and rental yields in this location
- Distance to CBD, schools, and transport
- Local market conditions and demand

IMPORTANT: Return null for any attribute you are not confident about. Only provide values when you have reasonable certainty based on the location and typical market data. It's better to return null than to guess incorrectly.

Return a JSON object with the following fields:

{{
    ""imageUrl"": ""A real, publicly accessible image URL of this specific property or a representative property image from the suburb. Use images from domain.com.au, realestate.com.au, or similar Australian property sites if possible. Return null if no suitable image can be found."",
    ""suburb"": ""The suburb name extracted from the address"",
    ""propertyType"": ""House|Unit|Townhouse|Villa|Duplex|Land"",
    ""landOwnership"": ""Freehold|Strata|Leasehold"",
    ""hasClearTitle"": true,
    ""hasEncumbrances"": false,
    ""zoning"": ""Residential|Commercial|Industrial|Mixed|Rural"",
    ""locationCategory"": ""Metro|Regional|Rural"",
    ""distanceToCbdKm"": 0,
    ""schoolZoneQuality"": ""Top-tier|Good|Average"",
    ""distanceToPublicTransportMeters"": 0,
    ""rentalYieldPercentage"": 0.0,
    ""capitalGrowthPercentage"": 0.0,
    ""vacancyRatePercentage"": 0.0,
    ""localDemand"": ""High|Medium|Low"",
    ""hasStructuralIssues"": false,
    ""propertyAgeYears"": 0,
    ""hasMajorDefects"": false,
    ""maintenanceLevel"": ""Minimal|Moderate|Extensive"",
    ""meetsCurrentBuildingCodes"": true,
    ""hasRequiredCertificates"": true,
    ""hasLongTermTenants"": false,
    ""hasReliablePaymentHistory"": true,
    ""leaseRemainingMonths"": 0,
    ""hasConsistentRentalHistory"": true,
    ""cashFlowCoverageRatio"": 1.0,
    ""meetsServiceabilityRequirements"": true,
    ""loanToValueRatio"": 80.0,
    ""annualInsuranceCost"": 0.0,
    ""suitableForCrossCollateral"": true,
    ""equityAvailable"": 0.0,
    ""eligibleForRefinance"": true,
    ""hasStableSaleHistory"": true,
    ""yearsSinceLastSale"": 0,
    ""daysOnMarket"": 0,
    ""hasStrongComparables"": true,
    ""isUniqueProperty"": false,
    ""acceptedByMajorLenders"": true,
    ""riskRating"": ""Low|Medium|High"",
    ""hasDevelopmentRisk"": false,
    ""fitsPortfolioDiversity"": true,
    ""viableForLongTermHold"": true,
    ""price"": 0,
    ""bedrooms"": 0,
    ""bathrooms"": 0,
    ""carSpaces"": 0,
    ""landSizeSqm"": 0
}}

Return ONLY the JSON object, no additional text or explanation.";

            var chatClient = _openAIClient.GetChatClient("gpt-4o-mini");
            var result = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("You are an expert Australian property analyst with comprehensive knowledge of property markets across Australia. Provide accurate property analysis data based on location, suburb characteristics, and current market conditions. Always return valid JSON."),
                new UserChatMessage(prompt)
            ]);

            var jsonResponse = result.Value.Content[0].Text?.Trim();

            if (string.IsNullOrEmpty(jsonResponse))
            {
                Console.WriteLine("OpenAI returned empty response");
                return null;
            }

            // Clean up the response - remove markdown code blocks if present
            jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

            Console.WriteLine($"OpenAI property data: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");

            // Parse the JSON response
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var extractedData = JsonSerializer.Deserialize<ExtractedPropertyData>(jsonResponse, options);

            if (extractedData == null)
            {
                Console.WriteLine("Failed to deserialize OpenAI response");
                return null;
            }

            // Map to PropertyDataDto
            return MapToPropertyDataDto(extractedData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting property data from OpenAI: {ex.Message}");
            return null;
        }
    }

    private static PropertyDataDto MapToPropertyDataDto(ExtractedPropertyData data)
    {
        return new PropertyDataDto
        {
            ImageUrl = data.ImageUrl,
            Suburb = data.Suburb,
            PropertyType = data.PropertyType,
            LandOwnership = data.LandOwnership,
            HasClearTitle = data.HasClearTitle,
            HasEncumbrances = data.HasEncumbrances,
            Zoning = data.Zoning,
            LocationCategory = data.LocationCategory,
            DistanceToCbdKm = data.DistanceToCbdKm,
            SchoolZoneQuality = data.SchoolZoneQuality,
            DistanceToPublicTransportMeters = data.DistanceToPublicTransportMeters,
            RentalYieldPercentage = data.RentalYieldPercentage,
            CapitalGrowthPercentage = data.CapitalGrowthPercentage,
            VacancyRatePercentage = data.VacancyRatePercentage,
            LocalDemand = data.LocalDemand,
            HasStructuralIssues = data.HasStructuralIssues,
            PropertyAgeYears = data.PropertyAgeYears,
            HasMajorDefects = data.HasMajorDefects,
            MaintenanceLevel = data.MaintenanceLevel,
            MeetsCurrentBuildingCodes = data.MeetsCurrentBuildingCodes,
            HasRequiredCertificates = data.HasRequiredCertificates,
            HasLongTermTenants = data.HasLongTermTenants,
            HasReliablePaymentHistory = data.HasReliablePaymentHistory,
            LeaseRemainingMonths = data.LeaseRemainingMonths,
            HasConsistentRentalHistory = data.HasConsistentRentalHistory,
            CashFlowCoverageRatio = data.CashFlowCoverageRatio,
            MeetsServiceabilityRequirements = data.MeetsServiceabilityRequirements,
            LoanToValueRatio = data.LoanToValueRatio,
            AnnualInsuranceCost = data.AnnualInsuranceCost,
            SuitableForCrossCollateral = data.SuitableForCrossCollateral,
            EquityAvailable = data.EquityAvailable,
            EligibleForRefinance = data.EligibleForRefinance,
            HasStableSaleHistory = data.HasStableSaleHistory,
            YearsSinceLastSale = data.YearsSinceLastSale,
            DaysOnMarket = data.DaysOnMarket,
            HasStrongComparables = data.HasStrongComparables,
            IsUniqueProperty = data.IsUniqueProperty,
            AcceptedByMajorLenders = data.AcceptedByMajorLenders,
            RiskRating = data.RiskRating,
            HasDevelopmentRisk = data.HasDevelopmentRisk,
            FitsPortfolioDiversity = data.FitsPortfolioDiversity,
            ViableForLongTermHold = data.ViableForLongTermHold
        };
    }

    private class ExtractedPropertyData
    {
        public string? ImageUrl { get; set; }
        public string? Suburb { get; set; }
        public string? PropertyType { get; set; }
        public string? LandOwnership { get; set; }
        public bool? HasClearTitle { get; set; }
        public bool? HasEncumbrances { get; set; }
        public string? Zoning { get; set; }
        public string? LocationCategory { get; set; }
        public int? DistanceToCbdKm { get; set; }
        public string? SchoolZoneQuality { get; set; }
        public int? DistanceToPublicTransportMeters { get; set; }
        public decimal? RentalYieldPercentage { get; set; }
        public decimal? CapitalGrowthPercentage { get; set; }
        public decimal? VacancyRatePercentage { get; set; }
        public string? LocalDemand { get; set; }
        public bool? HasStructuralIssues { get; set; }
        public int? PropertyAgeYears { get; set; }
        public bool? HasMajorDefects { get; set; }
        public string? MaintenanceLevel { get; set; }
        public bool? MeetsCurrentBuildingCodes { get; set; }
        public bool? HasRequiredCertificates { get; set; }
        public bool? HasLongTermTenants { get; set; }
        public bool? HasReliablePaymentHistory { get; set; }
        public int? LeaseRemainingMonths { get; set; }
        public bool? HasConsistentRentalHistory { get; set; }
        public decimal? CashFlowCoverageRatio { get; set; }
        public bool? MeetsServiceabilityRequirements { get; set; }
        public decimal? LoanToValueRatio { get; set; }
        public decimal? AnnualInsuranceCost { get; set; }
        public bool? SuitableForCrossCollateral { get; set; }
        public decimal? EquityAvailable { get; set; }
        public bool? EligibleForRefinance { get; set; }
        public bool? HasStableSaleHistory { get; set; }
        public int? YearsSinceLastSale { get; set; }
        public int? DaysOnMarket { get; set; }
        public bool? HasStrongComparables { get; set; }
        public bool? IsUniqueProperty { get; set; }
        public bool? AcceptedByMajorLenders { get; set; }
        public string? RiskRating { get; set; }
        public bool? HasDevelopmentRisk { get; set; }
        public bool? FitsPortfolioDiversity { get; set; }
        public bool? ViableForLongTermHold { get; set; }
    }
}
