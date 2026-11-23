using Shop_API.DTOs.CategoryDTOs;
using Shop_API.DTOs.MediaDTOs;

namespace Shop_API.Logics.IServices.IProductServices
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<CategoryListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken = default);

        Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<CategoryDto> EditAsync(int id, EditCategoryDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
