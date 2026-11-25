using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IMediaServices;
using Shop_API.Models.MediaModels;
using System.Globalization;

namespace Shop_API.Logics.Services.MediaServices
{
    public class MediaService : IMediaService
    {
        private const int MaxFileNameLength = 255;
        private const int MaxPageSize = 100;
        private const string InUseMessage = "این تصویر در حال استفاده است و قابل حذف نیست.";
        private const string ConcurrencyMessage = "این تصویر هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.";

        private readonly AppDbContext _context;
        private readonly IImageService _imageService;

        public MediaService(AppDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        private static string GetSafeFileName(string? fileName)
        {
            var name = (fileName ?? string.Empty).Replace('\\', '/');

            name = name[(name.LastIndexOf('/') + 1)..].Trim();

            if (name.Length == 0)
            {
                name = "image";
            }

            if (name.Length > MaxFileNameLength)
            {
                name = name[..MaxFileNameLength];
            }

            return name;
        }

        private async Task TryDeleteFileAsync(string storageKey)
        {
            try
            {
                await _imageService.DeleteAsync(storageKey, CancellationToken.None);
            }
            catch
            {
            }
        }

        private Task<bool> IsUsedAsync(Guid id, CancellationToken cancellationToken)
        {
            return _context.Media.AnyAsync(m => m.Id == id && (m.BrandLogos.Any() || m.CategoryImages.Any() || m.ProductVariantImages.Any()), cancellationToken);
        }

        private Task UnmarkDeletionAsync(Guid id)
        {
            return _context.Media
                .Where(m => m.Id == id && m.IsDeleting)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.IsDeleting, false)
                    .SetProperty(m => m.DeletionRequestedAt, (DateTime?)null), CancellationToken.None);
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(ConcurrencyMessage);
            }
        }

        public async Task<MediaUploadDto> UploadAsync(IFormFile image, CancellationToken cancellationToken = default)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("لطفاً یک تصویر انتخاب کنید.");
            }

            var now = DateTime.UtcNow;

            var storageFolder = string.Create(CultureInfo.InvariantCulture, $"media/{now.Year}/{now.Month:00}");

            var saved = await _imageService.SaveAsync(image, storageFolder, cancellationToken);

            var media = new Media
            {
                StorageKey = saved.StorageKey,
                OriginalFileName = GetSafeFileName(image.FileName),
                ContentType = saved.ContentType,
                Size = saved.Size,
                Width = saved.Width,
                Height = saved.Height,
                CreatedAt = now
            };

            try
            {
                _context.Media.Add(media);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await TryDeleteFileAsync(saved.StorageKey);

                throw;
            }

            return new MediaUploadDto
            {
                Id = media.Id,
                OriginalFileName = media.OriginalFileName,
                ContentType = media.ContentType,
                Size = media.Size,
                Width = saved.Width,
                Height = saved.Height,
                CreatedAt = media.CreatedAt
            };
        }

        public async Task<PagedResult<MediaListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
        {
            if (page < 1)
            {
                throw new ArgumentException("شماره صفحه باید بزرگ‌تر یا مساوی ۱ باشد.");
            }

            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                throw new ArgumentException($"تعداد آیتم‌های هر صفحه باید بین ۱ تا {MaxPageSize} باشد.");
            }

            var skip = (long)(page - 1) * pageSize;

            if (skip > int.MaxValue)
            {
                throw new ArgumentException("شماره صفحه معتبر نیست.");
            }

            var query = _context.Media
                .AsNoTracking()
                .Where(m => !m.IsDeleting);

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(m => m.OriginalFileName.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .Select(m => new MediaListDto
                {
                    Id = m.Id,
                    OriginalFileName = m.OriginalFileName,
                    Size = m.Size,
                    Width = m.Width,
                    Height = m.Height,
                    CreatedAt = m.CreatedAt,
                    IsUsed = m.BrandLogos.Any() || m.CategoryImages.Any() || m.ProductVariantImages.Any()
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<MediaListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<MediaUploadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Media
                .AsNoTracking()
                .Where(m => m.Id == id && !m.IsDeleting)
                .Select(m => new MediaUploadDto
                {
                    Id = m.Id,
                    OriginalFileName = m.OriginalFileName,
                    ContentType = m.ContentType,
                    Size = m.Size,
                    Width = m.Width ?? 0,
                    Height = m.Height ?? 0,
                    CreatedAt = m.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(Stream Stream, string ContentType)?> OpenAsync(Guid id, bool thumbnail = false, CancellationToken cancellationToken = default)
        {
            var storageKey = await _context.Media
                .AsNoTracking()
                .Where(m => m.Id == id && !m.IsDeleting)
                .Select(m => m.StorageKey)
                .FirstOrDefaultAsync(cancellationToken);

            if (storageKey == null)
            {
                return null;
            }

            return await _imageService.OpenAsync(storageKey, thumbnail, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var media = await _context.Media.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
                ?? throw new NotFoundException("تصویر مورد نظر یافت نشد.");

            if (await IsUsedAsync(id, cancellationToken))
            {
                await UnmarkDeletionAsync(id);

                throw new ConflictException(InUseMessage);
            }

            if (!media.IsDeleting)
            {
                media.IsDeleting = true;
                media.DeletionRequestedAt = DateTime.UtcNow;

                await SaveAsync(cancellationToken);
            }

            if (await IsUsedAsync(id, CancellationToken.None))
            {
                await UnmarkDeletionAsync(id);

                throw new ConflictException(InUseMessage);
            }

            var storageKey = media.StorageKey;

            _context.Media.Remove(media);

            try
            {
                await _context.SaveChangesAsync(CancellationToken.None);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(ConcurrencyMessage);
            }
            catch (DbUpdateException)
            {
                await UnmarkDeletionAsync(id);

                throw new ConflictException(InUseMessage);
            }

            await TryDeleteFileAsync(storageKey);
        }

        public async Task DeleteIfUnusedAsync(IEnumerable<Guid> mediaIds)
        {
            foreach (var id in mediaIds.Distinct())
            {
                try
                {
                    var storageKey = await _context.Media
                        .AsNoTracking()
                        .Where(m => m.Id == id && !m.BrandLogos.Any() && !m.CategoryImages.Any() && !m.ProductVariantImages.Any())
                        .Select(m => m.StorageKey)
                        .FirstOrDefaultAsync(CancellationToken.None);

                    if (storageKey == null)
                    {
                        continue;
                    }

                    var deleted = await _context.Media
                        .Where(m => m.Id == id && !m.BrandLogos.Any() && !m.CategoryImages.Any() && !m.ProductVariantImages.Any())
                        .ExecuteDeleteAsync(CancellationToken.None);

                    if (deleted > 0)
                    {
                        await TryDeleteFileAsync(storageKey);
                    }
                }
                catch
                {
                }
            }
        }
    }
}