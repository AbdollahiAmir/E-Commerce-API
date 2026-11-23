using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.ProductDTOs;

namespace Shop_API.Logics.IServices.IProductServices
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<ProductListDto>> GetPagedAsync(int page, int pageSize, string? search, int? brandId, int? categoryId, bool? isActive, CancellationToken cancellationToken = default);

        Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ProductDto> EditAsync(int id, EditProductDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
