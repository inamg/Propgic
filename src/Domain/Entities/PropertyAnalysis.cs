using Propgic.Domain.Common;

namespace Propgic.Domain.Entities;

public class PropertyAnalysis : BaseEntity
{
    public string PropertyAddress { get; set; } = string.Empty;
    public string AnalyserType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    public string? AnalysisResult { get; set; }
    public decimal? AnalysisScore { get; set; }
    public string? Remarks { get; set; }
    public DateTime? CompletedAt { get; set; }
}
