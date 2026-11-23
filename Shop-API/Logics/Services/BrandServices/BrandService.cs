using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.BrandDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IBrandServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.BrandServices
{
    public class BrandService : IBrandService
    {
        private const int MaxPageSize = 100;
        private const int SlugMaxLength = 200;
        private const string MediaDeletedMessage = "تصویر انتخاب‌شده هم‌زمان حذف شد. لطفاً تصویر دیگری انتخاب کنید.";
        private const string HasProductsMessage = "این برند دارای محصول است و قابل حذف نیست. در صورت نیاز می‌توانید آن را غیرفعال کنید.";

        private readonly AppDbContext _context;

        public BrandService(AppDbContext context)
        {
            _context = context;
        }

        private static string? NormalizeDescription(string? description)
        {
            var value = description?.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static BrandDto ToDto(Brand brand)
        {
            return new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Slug = brand.Slug,
                Description = brand.Description,
                LogoMediaId = brand.LogoMediaId,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt
            };
        }

        private async Task EnsureMediaAvailableAsync(Guid mediaId, CancellationToken cancellationToken)
        {
            var exists = await _context.Media.AnyAsync(m => m.Id == mediaId && !m.IsDeleting, cancellationToken);

            if (!exists)
            {
                throw new ArgumentException("تصویر انتخاب‌شده یافت نشد یا در حال حذف است.");
            }
        }

        private async Task SaveAsync(string foreignKeyConflictMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این برند هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این نشانی (Slug) هم‌زمان توسط برند دیگری ثبت شد. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(foreignKeyConflictMessage);
            }
        }

        public async Task<BrandDto> CreateAsync(CreateBrandDto dto, CancellationToken cancellationToken = default)
        {
            var name = dto.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                throw new ArgumentException("نام برند الزامی است.");
            }

            if (dto.LogoMediaId.HasValue)
            {
                await EnsureMediaAvailableAsync(dto.LogoMediaId.Value, cancellationToken);
            }

            var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, name, candidate => _context.Brands.AnyAsync(b => b.Slug == candidate, cancellationToken), SlugMaxLength);

            var brand = new Brand
            {
                Name = name,
                Slug = slug,
                Description = NormalizeDescription(dto.Description),
                LogoMediaId = dto.LogoMediaId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Brands.Add(brand);

            await SaveAsync(MediaDeletedMessage, cancellationToken);

            return ToDto(brand);
        }

        public async Task<PagedResult<BrandListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
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

            var query = _context.Brands.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(b => b.Name.Contains(term) || b.Slug.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderBy(b => b.Name).ThenBy(b => b.Id).Skip((int)skip).Take(pageSize).Select(b => new BrandListDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Slug = b.Slug,
                    LogoMediaId = b.LogoMediaId,
                    IsActive = b.IsActive,
                    ProductCount = b.Products.Count()
                }).ToListAsync(cancellationToken);

            return new PagedResult<BrandListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<BrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Brands.AsNoTracking().Where(b => b.Id == id).Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Slug = b.Slug,
                    Description = b.Description,
                    LogoMediaId = b.LogoMediaId,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<BrandDto> EditAsync(int id, EditBrandDto dto, CancellationToken cancellationToken = default)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken) ?? throw new NotFoundException("برند مورد نظر یافت نشد.");

            if (dto.LogoMediaId.HasValue && dto.RemoveLogo)
            {
                throw new ArgumentException("نمی‌توان هم‌زمان لوگوی جدید تعیین کرد و لوگوی فعلی را حذف کرد.");
            }

            var changed = false;

            if (dto.Name != null)
            {
                var name = dto.Name.Trim();

                if (name.Length == 0)
                {
                    throw new ArgumentException("نام برند نمی‌تواند خالی باشد.");
                }

                if (name != brand.Name)
                {
                    brand.Name = name;

                    changed = true;
                }
            }

            if (dto.Slug != null)
            {
                var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, brand.Name, candidate => _context.Brands.AnyAsync(b => b.Slug == candidate && b.Id != brand.Id, cancellationToken), SlugMaxLength);

                if (slug != brand.Slug)
                {
                    brand.Slug = slug;

                    changed = true;
                }
            }

            if (dto.Description != null)
            {
                var description = NormalizeDescription(dto.Description);

                if (description != brand.Description)
                {
                    brand.Description = description;

                    changed = true;
                }
            }

            if (dto.RemoveLogo && brand.LogoMediaId != null)
            {
                brand.LogoMediaId = null;

                changed = true;
            }

            if (dto.LogoMediaId.HasValue && dto.LogoMediaId != brand.LogoMediaId)
            {
                await EnsureMediaAvailableAsync(dto.LogoMediaId.Value, cancellationToken);

                brand.LogoMediaId = dto.LogoMediaId;

                changed = true;
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != brand.IsActive)
            {
                brand.IsActive = dto.IsActive.Value;

                changed = true;
            }

            if (!changed)
            {
                return ToDto(brand);
            }

            brand.UpdatedAt = DateTime.UtcNow;

            await SaveAsync(MediaDeletedMessage, cancellationToken);

            return ToDto(brand);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken) ?? throw new NotFoundException("برند مورد نظر یافت نشد.");

            var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id, cancellationToken);

            if (hasProducts)
            {
                throw new ConflictException(HasProductsMessage);
            }

            _context.Brands.Remove(brand);

            await SaveAsync(HasProductsMessage, cancellationToken);
        }
    }
}
