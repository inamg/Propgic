using AutoMapper;
using Propgic.Application.DTOs;
using Propgic.Application.Interfaces;
using Propgic.Domain.Entities;
using Propgic.Domain.Interfaces;

namespace Propgic.Application.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = _mapper.Map<Product>(createProductDto);
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;

        var createdProduct = await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(createdProduct);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            return null;

        _mapper.Map(updateProductDto, existingProduct);
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(existingProduct);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return false;

        await _productRepository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
