using Propgic.Application.DTOs;

namespace Propgic.Application.Services.PropertyDataFetchers;

public interface IPropertyDataFetcher
{
    Task<PropertyDataDto?> FetchPropertyDataAsync(string propertyAddress);
    Task<PropertyDataDto?> FetchPropertyDataFromUrlAsync(string propertyUrl);
    int Priority { get; }
    string SourceName { get; }
}
