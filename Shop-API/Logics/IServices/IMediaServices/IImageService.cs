using Shop_API.DTOs.MediaDTOs.ImageDTOs;

namespace Shop_API.Logics.IServices.IMediaServices
{
    public interface IImageService
    {
        Task<ImageSaveResult> SaveAsync(IFormFile image, string storageFolder, CancellationToken cancellationToken = default);

        Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

        Task<(Stream Stream, string ContentType)?> OpenAsync(string storageKey, bool thumbnail = false, CancellationToken cancellationToken = default);
    }
}
