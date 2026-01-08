namespace Propgic.Application.DTOs;

public class PropertyAnalysisDto
{
    public Guid Id { get; set; }
    public string PropertyAddress { get; set; } = string.Empty;
    public string AnalyserType { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AnalysisResult { get; set; }
    public decimal? AnalysisScore { get; set; }
    public string? Remarks { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePropertyAnalysisDto
{
    public string PropertyAddress { get; set; } = string.Empty;
    public string AnalyserType { get; set; } = string.Empty;
}

public class UpdatePropertyAnalysisDto
{
    public string Status { get; set; } = string.Empty;
    public string? AnalysisResult { get; set; }
    public decimal? AnalysisScore { get; set; }
    public string? Remarks { get; set; }
}

public class CreatePropertyAnalysisByUrlDto
{
    public string PropertyUrl { get; set; } = string.Empty;
    public string AnalyserType { get; set; } = string.Empty;
}
