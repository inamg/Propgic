using AutoMapper;
using Propgic.Application.DTOs;
using Propgic.Application.Interfaces;
using Propgic.Domain.Entities;
using Propgic.Domain.Interfaces;

namespace Propgic.Application.Services;

public class PropertyAnalyserService : IPropertyAnalyserService
{
    private readonly IRepository<PropertyAnalysis> _propertyAnalysisRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPropertyAnalyser> _analysers;

    public PropertyAnalyserService(
        IRepository<PropertyAnalysis> propertyAnalysisRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEnumerable<IPropertyAnalyser> analysers)
    {
        _propertyAnalysisRepository = propertyAnalysisRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _analysers = analysers;
    }

    public async Task<PropertyAnalysisDto?> GetAnalysisByIdAsync(Guid id)
    {
        var analysis = await _propertyAnalysisRepository.GetByIdAsync(id);
        return analysis == null ? null : _mapper.Map<PropertyAnalysisDto>(analysis);
    }

    public async Task<IEnumerable<PropertyAnalysisDto>> GetAllAnalysesAsync()
    {
        var analyses = await _propertyAnalysisRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PropertyAnalysisDto>>(analyses);
    }

    public async Task<IEnumerable<PropertyAnalysisDto>> GetAnalysesByTypeAsync(string analyserType)
    {
        var analyses = await _propertyAnalysisRepository.FindAsync(a => a.AnalyserType == analyserType);
        return _mapper.Map<IEnumerable<PropertyAnalysisDto>>(analyses);
    }

    public async Task<PropertyAnalysisDto> CreateAnalysisAsync(CreatePropertyAnalysisDto createDto)
    {
        var analysis = _mapper.Map<PropertyAnalysis>(createDto);
        analysis.Id = Guid.NewGuid();
        analysis.CreatedAt = DateTime.UtcNow;
        analysis.Status = "Pending";

        var createdAnalysis = await _propertyAnalysisRepository.AddAsync(analysis);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PropertyAnalysisDto>(createdAnalysis);
    }

    public async Task<PropertyAnalysisDto> CreateAnalysisByUrlAsync(CreatePropertyAnalysisByUrlDto createDto)
    {
        // Store the URL in PropertyAddress field
        var analysis = new PropertyAnalysis
        {
            Id = Guid.NewGuid(),
            PropertyAddress = createDto.PropertyUrl,
            AnalyserType = createDto.AnalyserType,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        var createdAnalysis = await _propertyAnalysisRepository.AddAsync(analysis);
        await _unitOfWork.SaveChangesAsync();

        // Automatically run the analysis
        return await RunAnalysisAsync(createdAnalysis.Id)
            ?? _mapper.Map<PropertyAnalysisDto>(createdAnalysis);
    }

    public async Task<PropertyAnalysisDto?> UpdateAnalysisAsync(Guid id, UpdatePropertyAnalysisDto updateDto)
    {
        var existingAnalysis = await _propertyAnalysisRepository.GetByIdAsync(id);
        if (existingAnalysis == null)
            return null;

        _mapper.Map(updateDto, existingAnalysis);
        existingAnalysis.UpdatedAt = DateTime.UtcNow;

        await _propertyAnalysisRepository.UpdateAsync(existingAnalysis);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PropertyAnalysisDto>(existingAnalysis);
    }

    public async Task<bool> DeleteAnalysisAsync(Guid id)
    {
        var analysis = await _propertyAnalysisRepository.GetByIdAsync(id);
        if (analysis == null)
            return false;

        await _propertyAnalysisRepository.DeleteAsync(analysis);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<PropertyAnalysisDto?> RunAnalysisAsync(Guid id)
    {
        var analysis = await _propertyAnalysisRepository.GetByIdAsync(id);
        if (analysis == null)
            return null;

        // Find the appropriate analyser
        var analyser = _analysers.FirstOrDefault(a => a.AnalyserType == analysis.AnalyserType);
        if (analyser == null)
        {
            analysis.Status = "Failed";
            analysis.Remarks = $"No analyser found for type: {analysis.AnalyserType}";
            await _propertyAnalysisRepository.UpdateAsync(analysis);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PropertyAnalysisDto>(analysis);
        }

        // Run the analysis
        var analysedProperty = await analyser.AnalyseAsync(analysis);

        // Update the database
        await _propertyAnalysisRepository.UpdateAsync(analysedProperty);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PropertyAnalysisDto>(analysedProperty);
    }
}
