using Propgic.Application.DTOs;

namespace Propgic.Application.Interfaces;

public interface IPropertyAnalyserService
{
    Task<PropertyAnalysisDto?> GetAnalysisByIdAsync(Guid id);
    Task<IEnumerable<PropertyAnalysisDto>> GetAllAnalysesAsync();
    Task<IEnumerable<PropertyAnalysisDto>> GetAnalysesByTypeAsync(string analyserType);
    Task<PropertyAnalysisDto> CreateAnalysisAsync(CreatePropertyAnalysisDto createDto);
    Task<PropertyAnalysisDto> CreateAnalysisByUrlAsync(CreatePropertyAnalysisByUrlDto createDto);
    Task<PropertyAnalysisDto?> UpdateAnalysisAsync(Guid id, UpdatePropertyAnalysisDto updateDto);
    Task<bool> DeleteAnalysisAsync(Guid id);
    Task<PropertyAnalysisDto?> RunAnalysisAsync(Guid id);
}
