using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.DTOs.ProductDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.IMediaServices;
using Shop_API.Logics.IServices.IProductServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private const int MaxPageSize = 100;
        private const int SlugMaxLength = 250;
        private const int MaxCategoriesPerProduct = 10;
        private const string ReferenceDeletedMessage = "برند یا دسته‌بندی انتخاب‌شده هم‌زمان حذف شد. لطفاً دوباره تلاش کنید.";
        private const string DeleteBlockedMessage = "این محصول به داده‌ی دیگری وابسته است و قابل حذف نیست.";

        private readonly AppDbContext _context;
        private readonly IMediaService _mediaService;

        public ProductService(AppDbContext context, IMediaService mediaService)
        {
            _context = context;
            _mediaService = mediaService;
        }

        private async Task<CategoryTree> LoadTreeAsync(CancellationToken cancellationToken)
        {
            var rows = await _context.Categories.AsNoTracking().Select(c => new { c.Id, c.ParentCategoryId }).ToListAsync(cancellationToken);

            return new CategoryTree(rows.Select(r => (r.Id, r.ParentCategoryId)));
        }

        private async Task<List<Category>> LoadCategoriesAsync(List<int> ids, CancellationToken cancellationToken)
        {
            if (ids.Count == 0)
            {
                throw new ArgumentException("حداقل یک دسته‌بندی باید انتخاب شود.");
            }

            if (ids.Count > MaxCategoriesPerProduct)
            {
                throw new ArgumentException($"هر محصول حداکثر می‌تواند در {MaxCategoriesPerProduct} دسته‌بندی باشد.");
            }

            var categories = await _context.Categories.Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken);

            if (categories.Count != ids.Count)
            {
                throw new ArgumentException("یک یا چند دسته‌بندی انتخاب‌شده یافت نشد.");
            }

            return categories;
        }

        private async Task EnsureBrandExistsAsync(int brandId, CancellationToken cancellationToken)
        {
            var exists = await _context.Brands.AnyAsync(b => b.Id == brandId, cancellationToken);

            if (!exists)
            {
                throw new ArgumentException("برند انتخاب‌شده یافت نشد.");
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
                throw new ConflictException("این محصول هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این نشانی (Slug) هم‌زمان توسط محصول دیگری ثبت شد. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(foreignKeyConflictMessage);
            }
        }

        private async Task<ProductDto> GetRequiredDtoAsync(int id, CancellationToken cancellationToken)
        {
            return await GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
        {
            var name = dto.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                throw new ArgumentException("نام محصول الزامی است.");
            }

            var categories = await LoadCategoriesAsync((dto.CategoryIds ?? []).Distinct().ToList(), cancellationToken);

            if (dto.BrandId.HasValue)
            {
                await EnsureBrandExistsAsync(dto.BrandId.Value, cancellationToken);
            }

            var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, name, candidate => _context.Products.AnyAsync(p => p.Slug == candidate, cancellationToken), SlugMaxLength);

            var product = new Product
            {
                Name = name,
                Slug = slug,
                BrandId = dto.BrandId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var category in categories)
            {
                product.Categories.Add(category);
            }

            _context.Products.Add(product);

            await SaveAsync(ReferenceDeletedMessage, cancellationToken);

            return await GetRequiredDtoAsync(product.Id, cancellationToken);
        }

        public async Task<PagedResult<ProductListDto>> GetPagedAsync(int page, int pageSize, string? search, int? brandId, int? categoryId, bool? isActive, CancellationToken cancellationToken = default)
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

            var query = _context.Products.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Name.Contains(term) || p.Slug.Contains(term));
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            if (categoryId.HasValue)
            {
                var tree = await LoadTreeAsync(cancellationToken);

                if (!tree.Contains(categoryId.Value))
                {
                    throw new ArgumentException("دسته‌بندی انتخاب‌شده برای فیلتر یافت نشد.");
                }

                var categoryIds = tree.GetDescendantIds(categoryId.Value);

                categoryIds.Add(categoryId.Value);

                query = query.Where(p => p.Categories.Any(c => categoryIds.Contains(c.Id)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Skip((int)skip).Take(pageSize).Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                IsActive = p.IsActive,
                VariantCount = p.Variants.Count(),
                CreatedAt = p.CreatedAt
            }).ToListAsync(cancellationToken);

            return new PagedResult<ProductListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Products.AsNoTracking().Where(p => p.Id == id).Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BrandId = p.BrandId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                Categories = p.Categories.OrderBy(c => c.Name).Select(c => new ProductCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                }).ToList(),
                IsActive = p.IsActive,
                VariantCount = p.Variants.Count(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ProductDto> EditAsync(int id, EditProductDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == id, cancellationToken) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

            if (dto.BrandId.HasValue && dto.RemoveBrand)
            {
                throw new ArgumentException("نمی‌توان هم‌زمان برند جدید تعیین کرد و برند فعلی را حذف کرد.");
            }

            var changed = false;

            if (dto.Name != null)
            {
                var name = dto.Name.Trim();

                if (name.Length == 0)
                {
                    throw new ArgumentException("نام محصول نمی‌تواند خالی باشد.");
                }

                if (name != product.Name)
                {
                    product.Name = name;

                    changed = true;
                }
            }

            if (dto.Slug != null)
            {
                var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, product.Name, candidate => _context.Products.AnyAsync(p => p.Slug == candidate && p.Id != product.Id, cancellationToken), SlugMaxLength);

                if (slug != product.Slug)
                {
                    product.Slug = slug;

                    changed = true;
                }
            }

            if (dto.RemoveBrand && product.BrandId != null)
            {
                product.BrandId = null;

                changed = true;
            }

            if (dto.BrandId.HasValue && dto.BrandId != product.BrandId)
            {
                await EnsureBrandExistsAsync(dto.BrandId.Value, cancellationToken);

                product.BrandId = dto.BrandId;

                changed = true;
            }

            if (dto.CategoryIds != null)
            {
                var ids = dto.CategoryIds.Distinct().ToList();

                var categories = await LoadCategoriesAsync(ids, cancellationToken);

                var currentIds = product.Categories.Select(c => c.Id).ToHashSet();

                if (!currentIds.SetEquals(ids))
                {
                    foreach (var removed in product.Categories.Where(c => !ids.Contains(c.Id)).ToList())
                    {
                        product.Categories.Remove(removed);
                    }

                    foreach (var added in categories.Where(c => !currentIds.Contains(c.Id)))
                    {
                        product.Categories.Add(added);
                    }

                    changed = true;
                }
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != product.IsActive)
            {
                product.IsActive = dto.IsActive.Value;

                changed = true;
            }

            if (changed)
            {
                product.UpdatedAt = DateTime.UtcNow;

                await SaveAsync(ReferenceDeletedMessage, cancellationToken);
            }

            return await GetRequiredDtoAsync(product.Id, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken) ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

            var mediaIds = await _context.ProductVariantImages
                .AsNoTracking()
                .Where(i => i.ProductVariant.ProductId == id)
                .Select(i => i.MediaId)
                .Distinct()
                .ToListAsync(cancellationToken);

            _context.Products.Remove(product);

            await SaveAsync(DeleteBlockedMessage, cancellationToken);

            if (mediaIds.Count > 0)
            {
                await _mediaService.DeleteIfUnusedAsync(mediaIds);
            }
        }
    }
}