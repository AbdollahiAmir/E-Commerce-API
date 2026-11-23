using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.VariantDTOs;

namespace Shop_API.Logics.IServices.IProductServices
{
    public interface IVariantService
    {
        Task<VariantDto> CreateAsync(CreateVariantDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<VariantListDto>> GetPagedAsync(int page, int pageSize, string? search, int? productId, bool? isActive, CancellationToken cancellationToken = default);

        Task<VariantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<VariantDto> EditAsync(int id, EditVariantDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
