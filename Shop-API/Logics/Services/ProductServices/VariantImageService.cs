using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.VariantDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.ProductServices
{
    public class VariantImageService : IVariantImageService
    {
        private const int MaxImagesPerVariant = 10;
        private const string MediaDeletedMessage = "تصویر انتخاب‌شده هم‌زمان حذف شد. لطفاً تصویر دیگری انتخاب کنید.";

        private readonly AppDbContext _context;

        public VariantImageService(AppDbContext context)
        {
            _context = context;
        }

        private static string? NormalizeAltText(string? altText)
        {
            var value = altText?.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static int ValidateDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
            {
                throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد.");
            }

            return displayOrder;
        }

        private static VariantImageDto Map(ProductVariantImage image)
        {
            return new VariantImageDto
            {
                Id = image.Id,
                VariantId = image.ProductVariantId,
                MediaId = image.MediaId,
                AltText = image.AltText,
                DisplayOrder = image.DisplayOrder,
                IsMain = image.IsMain,
                CreatedAt = image.CreatedAt
            };
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این تصویر هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این تصویر قبلاً ثبت شده است یا تصویر اصلی هم‌زمان تغییر کرد. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(MediaDeletedMessage);
            }
        }

        private async Task LockVariantAsync(int variantId, CancellationToken cancellationToken)
        {
            var locked = await _context.Database
                .SqlQuery<int>($"SELECT [Id] AS [Value] FROM [ProductVariants] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {variantId}")
                .ToListAsync(cancellationToken);

            if (locked.Count == 0)
            {
                throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");
            }
        }

        private Task DemoteOtherMainImagesAsync(int variantId, int? exceptImageId, CancellationToken cancellationToken)
        {
            return _context.ProductVariantImages
                .Where(i => i.ProductVariantId == variantId && i.IsMain && (exceptImageId == null || i.Id != exceptImageId))
                .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsMain, false), cancellationToken);
        }

        private async Task<ProductVariantImage> GetImageAsync(int variantId, int imageId, CancellationToken cancellationToken)
        {
            return await _context.ProductVariantImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ProductVariantId == variantId, cancellationToken)
                ?? throw new NotFoundException("تصویر مورد نظر برای این نوع محصول یافت نشد.");
        }

        public async Task<VariantImageDto> AddAsync(int variantId, AddVariantImageDto dto, CancellationToken cancellationToken = default)
        {
            var displayOrder = dto.DisplayOrder.HasValue ? ValidateDisplayOrder(dto.DisplayOrder.Value) : (int?)null;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockVariantAsync(variantId, cancellationToken);

            var mediaAvailable = await _context.Media.AnyAsync(m => m.Id == dto.MediaId && !m.IsDeleting, cancellationToken);

            if (!mediaAvailable)
            {
                throw new ArgumentException("تصویر انتخاب‌شده یافت نشد یا در حال حذف است.");
            }

            var existing = await _context.ProductVariantImages
                .AsNoTracking()
                .Where(i => i.ProductVariantId == variantId)
                .Select(i => new { i.MediaId, i.DisplayOrder })
                .ToListAsync(cancellationToken);

            var count = existing.Count;

            if (count >= MaxImagesPerVariant)
            {
                throw new ConflictException($"هر نوع محصول حداکثر می‌تواند {MaxImagesPerVariant} تصویر داشته باشد.");
            }

            if (existing.Any(i => i.MediaId == dto.MediaId))
            {
                throw new ConflictException("این تصویر قبلاً برای این نوع محصول ثبت شده است.");
            }

            var isMain = dto.IsMain || count == 0;

            if (isMain && count > 0)
            {
                await DemoteOtherMainImagesAsync(variantId, null, cancellationToken);
            }

            displayOrder ??= count == 0 ? 0 : existing.Max(i => i.DisplayOrder) + 1;

            var image = new ProductVariantImage
            {
                ProductVariantId = variantId,
                MediaId = dto.MediaId,
                AltText = NormalizeAltText(dto.AltText),
                DisplayOrder = displayOrder.Value,
                IsMain = isMain,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductVariantImages.Add(image);

            await SaveAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Map(image);
        }

        public async Task<IReadOnlyList<VariantImageDto>> GetAllAsync(int variantId, CancellationToken cancellationToken = default)
        {
            var variantExists = await _context.ProductVariants.AnyAsync(v => v.Id == variantId, cancellationToken);

            if (!variantExists)
            {
                throw new NotFoundException("نوع محصول مورد نظر یافت نشد.");
            }

            return await _context.ProductVariantImages
                .AsNoTracking()
                .Where(i => i.ProductVariantId == variantId)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.Id)
                .Select(i => new VariantImageDto
                {
                    Id = i.Id,
                    VariantId = i.ProductVariantId,
                    MediaId = i.MediaId,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsMain = i.IsMain,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<VariantImageDto> EditAsync(int variantId, int imageId, EditVariantImageDto dto, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockVariantAsync(variantId, cancellationToken);

            var image = await GetImageAsync(variantId, imageId, cancellationToken);

            var changed = false;

            if (dto.AltText != null)
            {
                var altText = NormalizeAltText(dto.AltText);

                if (altText != image.AltText)
                {
                    image.AltText = altText;

                    changed = true;
                }
            }

            if (dto.DisplayOrder.HasValue)
            {
                var displayOrder = ValidateDisplayOrder(dto.DisplayOrder.Value);

                if (displayOrder != image.DisplayOrder)
                {
                    image.DisplayOrder = displayOrder;

                    changed = true;
                }
            }

            if (dto.IsMain.HasValue)
            {
                if (!dto.IsMain.Value && image.IsMain)
                {
                    throw new ArgumentException("برای تغییر تصویر اصلی، یکی از تصاویر دیگر را به‌عنوان تصویر اصلی انتخاب کنید.");
                }

                if (dto.IsMain.Value && !image.IsMain)
                {
                    await DemoteOtherMainImagesAsync(variantId, image.Id, cancellationToken);

                    image.IsMain = true;

                    changed = true;
                }
            }

            if (changed)
            {
                await SaveAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }

            return Map(image);
        }

        public async Task DeleteAsync(int variantId, int imageId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await LockVariantAsync(variantId, cancellationToken);

            var image = await GetImageAsync(variantId, imageId, cancellationToken);

            var wasMain = image.IsMain;

            _context.ProductVariantImages.Remove(image);

            await SaveAsync(cancellationToken);

            if (wasMain)
            {
                var nextId = await _context.ProductVariantImages
                    .Where(i => i.ProductVariantId == variantId)
                    .OrderBy(i => i.DisplayOrder)
                    .ThenBy(i => i.Id)
                    .Select(i => (int?)i.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextId.HasValue)
                {
                    await _context.ProductVariantImages
                        .Where(i => i.Id == nextId.Value)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsMain, true), cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}