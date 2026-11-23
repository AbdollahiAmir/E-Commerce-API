using Shop_API.DTOs.VariantDTOs;

namespace Shop_API.Logics.IServices.IProductServices
{
    public interface IVariantImageService
    {
        Task<VariantImageDto> AddAsync(int variantId, AddVariantImageDto dto, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VariantImageDto>> GetAllAsync(int variantId, CancellationToken cancellationToken = default);

        Task<VariantImageDto> EditAsync(int variantId, int imageId, EditVariantImageDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int variantId, int imageId, CancellationToken cancellationToken = default);
    }
}