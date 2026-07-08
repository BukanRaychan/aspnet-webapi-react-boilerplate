using AutoMapper;
using WebApi.DTOs.Common;
using WebApi.DTOs.ProductDtos;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ProductResponseDto>> GetPagedAsync(PaginationQueryDto pagination)
    {
        (List<Product> items, int totalCount) = await _productRepository.GetPagedAsync(pagination);
        List<ProductResponseDto> response = _mapper.Map<List<ProductResponseDto>>(items);
        foreach (ProductResponseDto productResponse in response)
        {
            productResponse.Stock = items.First(p => p.Id == productResponse.Id).UnitProducts.Count;
        }

        return PagedResponse<ProductResponseDto>.Create(response, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        Product? product = await _productRepository.GetByIdAsync(id);
        if (product == null) {
            return null;
        }

        ProductResponseDto productResponse = _mapper.Map<ProductResponseDto>(product);
        productResponse.Stock = await _productRepository.GetStockCountAsync(id);
        return productResponse;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        Product product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        Product created = await _productRepository.CreateAsync(product);
        return _mapper.Map<ProductResponseDto>(created);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        Product? existing = await _productRepository.GetByIdAsync(id);
        if (existing == null) {
            return null;
        }
        _mapper.Map(dto, existing);
        Product? updated = await _productRepository.UpdateAsync(existing);
        return _mapper.Map<ProductResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }
}