using AutoMapper;
using WebApi.DTOs.Common;
using WebApi.DTOs.UnitProductDtos;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Services;

public class UnitProductService : IUnitProductService
{
    private readonly IUnitProductRepository _unitProductRepository;

    private readonly IMapper _mapper;

    public UnitProductService(IUnitProductRepository unitProductRepository, IMapper mapper)
    {
        _unitProductRepository = unitProductRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<UnitProductResponseDto>> GetPagedAsync(PaginationQueryDto pagination)
    {
        (List<UnitProduct> items, int totalCount) = await _unitProductRepository.GetPagedAsync(pagination);
        List<UnitProductResponseDto> response = _mapper.Map<List<UnitProductResponseDto>>(items);
        return PagedResponse<UnitProductResponseDto>.Create(response, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<UnitProductResponseDto?> GetByIdAsync(int id)
    {
        UnitProduct? unitProduct = await _unitProductRepository.GetByIdAsync(id);
        if (unitProduct == null) {
            return null;
        }
        return _mapper.Map<UnitProductResponseDto>(unitProduct);
    }

    public async Task<List<UnitProductResponseDto>> GetByProductIdAsync(int productId)
    {
        List<UnitProduct> unitProducts = await _unitProductRepository.GetByProductIdAsync(productId);
        return _mapper.Map<List<UnitProductResponseDto>>(unitProducts);
    }

    public async Task<UnitProductResponseDto> CreateAsync(string userId, CreateUnitProductDto dto)
    {
        UnitProduct unitProduct = _mapper.Map<UnitProduct>(dto);
        unitProduct.UserId = userId;
        unitProduct.CreatedAt = DateTime.UtcNow;
        UnitProduct created = await _unitProductRepository.CreateAsync(unitProduct);
        return _mapper.Map<UnitProductResponseDto>(created);
    }

    public async Task<UnitProductResponseDto?> UpdateAsync(int id, UpdateUnitProductDto dto)
    {
        UnitProduct? existing = await _unitProductRepository.GetByIdAsync(id);
        if (existing == null) {
            return null;
        }

        _mapper.Map(dto, existing);
        UnitProduct? updated = await _unitProductRepository.UpdateAsync(existing);
        return _mapper.Map<UnitProductResponseDto>(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _unitProductRepository.DeleteAsync(id);
    }
}