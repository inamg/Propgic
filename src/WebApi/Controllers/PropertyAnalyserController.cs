using Propgic.Application.DTOs;
using Propgic.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Propgic.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyAnalyserController : ControllerBase
{
    private readonly IPropertyAnalyserService _propertyAnalyserService;
    private readonly ILogger<PropertyAnalyserController> _logger;

    public PropertyAnalyserController(
        IPropertyAnalyserService propertyAnalyserService,
        ILogger<PropertyAnalyserController> logger)
    {
        _propertyAnalyserService = propertyAnalyserService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyAnalysisDto>>> GetAllAnalyses()
    {
        try
        {
            var analyses = await _propertyAnalyserService.GetAllAnalysesAsync();
            return Ok(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property analyses");
            return StatusCode(500, "An error occurred while retrieving property analyses");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyAnalysisDto>> GetAnalysis(Guid id)
    {
        try
        {
            var analysis = await _propertyAnalyserService.GetAnalysisByIdAsync(id);
            if (analysis == null)
                return NotFound();

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while retrieving the property analysis");
        }
    }

    [HttpGet("type/{analyserType}")]
    public async Task<ActionResult<IEnumerable<PropertyAnalysisDto>>> GetAnalysesByType(string analyserType)
    {
        try
        {
            var analyses = await _propertyAnalyserService.GetAnalysesByTypeAsync(analyserType);
            return Ok(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property analyses by type {AnalyserType}", analyserType);
            return StatusCode(500, "An error occurred while retrieving property analyses by type");
        }
    }

    [HttpPost]
    public async Task<ActionResult<PropertyAnalysisDto>> CreateAnalysis(CreatePropertyAnalysisDto createDto)
    {
        try
        {
            var analysis = await _propertyAnalyserService.CreateAnalysisAsync(createDto);
            return CreatedAtAction(nameof(GetAnalysis), new { id = analysis.Id }, analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property analysis");
            return StatusCode(500, "An error occurred while creating the property analysis");
        }
    }

    [HttpPost("by-url")]
    public async Task<ActionResult<PropertyAnalysisDto>> CreateAnalysisByUrl(CreatePropertyAnalysisByUrlDto createDto)
    {
        try
        {
            var analysis = await _propertyAnalyserService.CreateAnalysisByUrlAsync(createDto);
            return CreatedAtAction(nameof(GetAnalysis), new { id = analysis.Id }, analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property analysis by URL");
            return StatusCode(500, "An error occurred while creating the property analysis by URL");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PropertyAnalysisDto>> UpdateAnalysis(Guid id, UpdatePropertyAnalysisDto updateDto)
    {
        try
        {
            var analysis = await _propertyAnalyserService.UpdateAnalysisAsync(id, updateDto);
            if (analysis == null)
                return NotFound();

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while updating the property analysis");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnalysis(Guid id)
    {
        try
        {
            var result = await _propertyAnalyserService.DeleteAnalysisAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while deleting the property analysis");
        }
    }

    [HttpPost("{id}/run")]
    public async Task<ActionResult<PropertyAnalysisDto>> RunAnalysis(Guid id)
    {
        try
        {
            var analysis = await _propertyAnalyserService.RunAnalysisAsync(id);
            if (analysis == null)
                return NotFound();

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running property analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while running the property analysis");
        }
    }
}
