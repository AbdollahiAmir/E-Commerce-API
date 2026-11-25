using Shop_API.DTOs.MediaDTOs;

namespace Shop_API.Logics.IServices.IMediaServices
{
    public interface IMediaService
    {
        Task<MediaUploadDto> UploadAsync(IFormFile image, CancellationToken cancellationToken = default);

        Task<PagedResult<MediaListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);

        Task<MediaUploadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(Stream Stream, string ContentType)?> OpenAsync(Guid id, bool thumbnail = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}