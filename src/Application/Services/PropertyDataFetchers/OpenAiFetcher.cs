using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public class OpenAiFetcher : IPropertyDataFetcher
{
    private readonly ChatGptService _chatGptService;

    public int Priority => 0; // Highest priority for address-based searches
    public string SourceName => "OpenAI";

    public OpenAiFetcher(ChatGptService chatGptService)
    {
        _chatGptService = chatGptService;
    }

    public async Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress)
    {
        try
        {
            Console.WriteLine($"Fetching property data from OpenAI for: {propertyAddress}");

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
            Console.WriteLine($"Error fetching from OpenAI: {ex.Message}");
            return null;
        }
    }

    public async Task<PropertyDataDto?> FetchPropertyDataFromUrlAsync(string propertyUrl)
    {
        // OpenAI fetcher doesn't handle URL-based fetching directly
        // It extracts address from URL and fetches data based on address
        try
        {
            Console.WriteLine($"OpenAI fetcher extracting address from URL: {propertyUrl}");

            var address = ExtractAddressFromUrl(propertyUrl);
            if (!string.IsNullOrEmpty(address))
            {
                return await FetchPropertyDataAsync(address);
            }

            Console.WriteLine($"Could not extract address from URL: {propertyUrl}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from URL via OpenAI: {ex.Message}");
            return null;
        }
    }

    private static string? ExtractAddressFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.Trim('/');

            // Remove common prefixes
            foreach (var prefix in new[] { "property-profile/", "property/", "listing/" })
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(prefix.Length);
            }

            // Convert URL slug to address format
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
}
