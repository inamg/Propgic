using Propgic.Domain.Entities;

namespace Propgic.Application.Interfaces;

public interface IPropertyAnalyser
{
    string AnalyserType { get; }
    Task<PropertyAnalysis> AnalyseAsync(PropertyAnalysis propertyAnalysis);
}
