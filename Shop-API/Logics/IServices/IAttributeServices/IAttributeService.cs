using Shop_API.DTOs.AttributeDTOs;
using Shop_API.DTOs.MediaDTOs;

namespace Shop_API.Logics.IServices.IAttributeServices
{
    public interface IAttributeService
    {
        Task<AttributeDto> CreateAsync(CreateAttributeDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<AttributeListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

        Task<AttributeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<AttributeDto> EditAsync(int id, EditAttributeDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
