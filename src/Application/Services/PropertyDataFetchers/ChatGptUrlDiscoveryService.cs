using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class ChatGptUrlDiscoveryService
{
    private readonly string _apiKey;
    private readonly AzureOpenAIClient? _azureClient;
    private readonly OpenAI.OpenAIClient? _openAIClient;
    private readonly bool _isAzure;
    private readonly string _azureDeploymentName;

    public ChatGptUrlDiscoveryService(string apiKey, string? azureEndpoint = null, string? azureDeploymentName = null)
    {
        _apiKey = apiKey;
        _azureDeploymentName = azureDeploymentName ?? "gpt-4";

        if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureDeploymentName))
        {
            _azureClient = new AzureOpenAIClient(new Uri(azureEndpoint), new AzureKeyCredential(apiKey));
            _isAzure = true;
        }
        else
        {
            _openAIClient = new OpenAI.OpenAIClient(apiKey);
            _isAzure = false;
        }
    }

    public async Task<string?> GetPropertyUrlAsync(string propertyAddress, string websiteName)
    {
        try
        {
            var prompt = $@"Given the property address: '{propertyAddress}'

Please construct or suggest the most likely property listing URL on {websiteName}.

Return ONLY the URL, nothing else. If you cannot determine a specific URL, return an empty string.";

            ChatCompletion completion;

            if (_isAzure && _azureClient != null)
            {
                var chatClient = _azureClient.GetChatClient("gpt-4");
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are a helpful assistant that constructs property listing URLs for Australian real estate websites."),
                    new UserChatMessage(prompt)
                ]);
            }
            else if (_openAIClient != null)
            {
                var chatClient = _openAIClient.GetChatClient("gpt-4o-mini");
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are a helpful assistant that constructs property listing URLs for Australian real estate websites."),
                    new UserChatMessage(prompt)
                ]);
            }
            else
            {
                return null;
            }

            var url = completion.Content[0].Text?.Trim();

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

Return a JSON object with the following fields. Use your best estimates based on the location and typical properties in that area:

{{
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

            ChatCompletion completion;

            if (_isAzure && _azureClient != null)
            {
                var chatClient = _azureClient.GetChatClient(_azureDeploymentName);
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are an expert Australian property analyst with comprehensive knowledge of property markets across Australia. Provide accurate property analysis data based on location, suburb characteristics, and current market conditions. Always return valid JSON."),
                    new UserChatMessage(prompt)
                ]);
            }
            else if (_openAIClient != null)
            {
                var chatClient = _openAIClient.GetChatClient("gpt-4o-mini");
                completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage("You are an expert Australian property analyst with comprehensive knowledge of property markets across Australia. Provide accurate property analysis data based on location, suburb characteristics, and current market conditions. Always return valid JSON."),
                    new UserChatMessage(prompt)
                ]);
            }
            else
            {
                return null;
            }

            var jsonResponse = completion.Content[0].Text?.Trim();

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

    private PropertyDataDto MapToPropertyDataDto(ExtractedPropertyData data)
    {
        return new PropertyDataDto
        {
            PropertyType = data.PropertyType ?? "House",
            LandOwnership = data.LandOwnership ?? "Freehold",
            HasClearTitle = data.HasClearTitle,
            HasEncumbrances = data.HasEncumbrances,
            Zoning = data.Zoning ?? "Residential",
            LocationCategory = data.LocationCategory ?? "Metro",
            DistanceToCbdKm = data.DistanceToCbdKm,
            SchoolZoneQuality = data.SchoolZoneQuality ?? "Average",
            DistanceToPublicTransportMeters = data.DistanceToPublicTransportMeters,
            RentalYieldPercentage = data.RentalYieldPercentage > 0 ? data.RentalYieldPercentage : CalculateDefaultRentalYield(data.Price, data.Bedrooms, data.LocationCategory),
            CapitalGrowthPercentage = data.CapitalGrowthPercentage > 0 ? data.CapitalGrowthPercentage : GetDefaultCapitalGrowth(data.LocationCategory),
            VacancyRatePercentage = data.VacancyRatePercentage > 0 ? data.VacancyRatePercentage : GetDefaultVacancyRate(data.LocationCategory),
            LocalDemand = data.LocalDemand ?? "Medium",
            HasStructuralIssues = data.HasStructuralIssues,
            PropertyAgeYears = data.PropertyAgeYears,
            HasMajorDefects = data.HasMajorDefects,
            MaintenanceLevel = data.MaintenanceLevel ?? "Minimal",
            MeetsCurrentBuildingCodes = data.MeetsCurrentBuildingCodes,
            HasRequiredCertificates = data.HasRequiredCertificates,
            HasLongTermTenants = data.HasLongTermTenants,
            HasReliablePaymentHistory = data.HasReliablePaymentHistory,
            LeaseRemainingMonths = data.LeaseRemainingMonths,
            HasConsistentRentalHistory = data.HasConsistentRentalHistory,
            CashFlowCoverageRatio = data.CashFlowCoverageRatio > 0 ? data.CashFlowCoverageRatio : 1.0m,
            MeetsServiceabilityRequirements = data.MeetsServiceabilityRequirements,
            LoanToValueRatio = data.LoanToValueRatio > 0 ? data.LoanToValueRatio : 80m,
            AnnualInsuranceCost = data.AnnualInsuranceCost > 0 ? data.AnnualInsuranceCost : CalculateInsuranceCost(data.Price, data.PropertyType),
            SuitableForCrossCollateral = data.SuitableForCrossCollateral,
            EquityAvailable = data.EquityAvailable > 0 ? data.EquityAvailable : data.Price * 0.2m,
            EligibleForRefinance = data.EligibleForRefinance,
            HasStableSaleHistory = data.HasStableSaleHistory,
            YearsSinceLastSale = data.YearsSinceLastSale,
            DaysOnMarket = data.DaysOnMarket,
            HasStrongComparables = data.HasStrongComparables,
            IsUniqueProperty = data.IsUniqueProperty,
            AcceptedByMajorLenders = data.AcceptedByMajorLenders,
            RiskRating = data.RiskRating ?? "Low",
            HasDevelopmentRisk = data.HasDevelopmentRisk,
            FitsPortfolioDiversity = data.FitsPortfolioDiversity,
            ViableForLongTermHold = data.ViableForLongTermHold
        };
    }

    private decimal CalculateDefaultRentalYield(decimal price, int bedrooms, string? locationCategory)
    {
        if (price <= 0) return 4.0m;

        decimal weeklyRent = (bedrooms, locationCategory ?? "Metro") switch
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
        return Math.Round((annualRent / price) * 100, 2);
    }

    private decimal GetDefaultCapitalGrowth(string? locationCategory)
    {
        return locationCategory switch
        {
            "Metro" => 6.5m,
            "Regional" => 4.5m,
            _ => 5.0m
        };
    }

    private decimal GetDefaultVacancyRate(string? locationCategory)
    {
        return locationCategory switch
        {
            "Metro" => 1.5m,
            "Regional" => 2.5m,
            _ => 2.0m
        };
    }

    private decimal CalculateInsuranceCost(decimal price, string? propertyType)
    {
        var baseRate = propertyType switch
        {
            "Unit" => 0.001m,
            "Townhouse" => 0.0012m,
            _ => 0.0015m
        };

        return Math.Round(price * baseRate, 0);
    }

    private class ExtractedPropertyData
    {
        public string? PropertyType { get; set; }
        public string? LandOwnership { get; set; }
        public bool HasClearTitle { get; set; } = true;
        public bool HasEncumbrances { get; set; }
        public string? Zoning { get; set; }
        public string? LocationCategory { get; set; }
        public int DistanceToCbdKm { get; set; }
        public string? SchoolZoneQuality { get; set; }
        public int DistanceToPublicTransportMeters { get; set; }
        public decimal RentalYieldPercentage { get; set; }
        public decimal CapitalGrowthPercentage { get; set; }
        public decimal VacancyRatePercentage { get; set; }
        public string? LocalDemand { get; set; }
        public bool HasStructuralIssues { get; set; }
        public int PropertyAgeYears { get; set; }
        public bool HasMajorDefects { get; set; }
        public string? MaintenanceLevel { get; set; }
        public bool MeetsCurrentBuildingCodes { get; set; } = true;
        public bool HasRequiredCertificates { get; set; } = true;
        public bool HasLongTermTenants { get; set; }
        public bool HasReliablePaymentHistory { get; set; } = true;
        public int LeaseRemainingMonths { get; set; }
        public bool HasConsistentRentalHistory { get; set; } = true;
        public decimal CashFlowCoverageRatio { get; set; }
        public bool MeetsServiceabilityRequirements { get; set; } = true;
        public decimal LoanToValueRatio { get; set; }
        public decimal AnnualInsuranceCost { get; set; }
        public bool SuitableForCrossCollateral { get; set; } = true;
        public decimal EquityAvailable { get; set; }
        public bool EligibleForRefinance { get; set; } = true;
        public bool HasStableSaleHistory { get; set; } = true;
        public int YearsSinceLastSale { get; set; }
        public int DaysOnMarket { get; set; }
        public bool HasStrongComparables { get; set; } = true;
        public bool IsUniqueProperty { get; set; }
        public bool AcceptedByMajorLenders { get; set; } = true;
        public string? RiskRating { get; set; }
        public bool HasDevelopmentRisk { get; set; }
        public bool FitsPortfolioDiversity { get; set; } = true;
        public bool ViableForLongTermHold { get; set; } = true;
        public decimal Price { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int CarSpaces { get; set; }
        public int LandSizeSqm { get; set; }
    }
}
