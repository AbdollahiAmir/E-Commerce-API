using Shop_API.DTOs.BrandDTOs;
using Shop_API.DTOs.MediaDTOs;

namespace Shop_API.Logics.IServices.IProductServices
{
    public interface IBrandService
    {
        Task<BrandDto> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<BrandListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

        Task<BrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<BrandDto> EditAsync(int id, EditBrandDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
