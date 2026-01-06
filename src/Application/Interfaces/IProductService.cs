using Propgic.Application.DTOs;

namespace Propgic.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(Guid id);
}
