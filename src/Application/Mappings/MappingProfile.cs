using AutoMapper;
using Propgic.Application.DTOs;
using Propgic.Domain.Entities;

namespace Propgic.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<PropertyAnalysis, PropertyAnalysisDto>().ReverseMap();
        CreateMap<CreatePropertyAnalysisDto, PropertyAnalysis>();
        CreateMap<UpdatePropertyAnalysisDto, PropertyAnalysis>();
    }
}
