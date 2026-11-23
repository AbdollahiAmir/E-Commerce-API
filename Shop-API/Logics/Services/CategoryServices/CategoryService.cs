using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.CategoryDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.Helpers;
using Shop_API.Logics.IServices.ICategoryServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private const int MaxPageSize = 100;
        private const int SlugMaxLength = 200;
        private const int MaxDepth = 5;
        private const string ReferenceDeletedMessage = "تصویر یا دسته‌بندی والد انتخاب‌شده هم‌زمان حذف شد. لطفاً دوباره تلاش کنید.";
        private const string DeleteConflictMessage = "هم‌زمان محصول یا زیردسته‌ای به این دسته‌بندی اضافه شد. لطفاً دوباره تلاش کنید.";
        private const string HasProductsMessage = "این دسته‌بندی دارای محصول است و قابل حذف نیست. در صورت نیاز می‌توانید آن را غیرفعال کنید.";

        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        private static string? NormalizeDescription(string? description)
        {
            var value = description?.Trim();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static CategoryDto Map(Category category, IReadOnlyList<CategoryBreadcrumbDto> breadcrumb)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageMediaId = category.ImageMediaId,
                ParentCategoryId = category.ParentCategoryId,
                Breadcrumb = breadcrumb,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private async Task<CategoryTree> LoadTreeAsync(CancellationToken cancellationToken)
        {
            var rows = await _context.Categories.AsNoTracking().Select(c => new { c.Id, c.ParentCategoryId }).ToListAsync(cancellationToken);

            return new CategoryTree(rows.Select(r => (r.Id, r.ParentCategoryId)));
        }

        private async Task<IReadOnlyList<CategoryBreadcrumbDto>> GetBreadcrumbAsync(int id, CancellationToken cancellationToken)
        {
            var tree = await LoadTreeAsync(cancellationToken);

            var ancestorIds = tree.GetAncestorIds(id);

            if (ancestorIds.Count == 0)
            {
                return [];
            }

            var items = await _context.Categories.AsNoTracking().Where(c => ancestorIds.Contains(c.Id)).Select(c => new CategoryBreadcrumbDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                }).ToListAsync(cancellationToken);

            var byId = items.ToDictionary(i => i.Id);

            return ancestorIds.Where(byId.ContainsKey).Select(a => byId[a]).ToList();
        }

        private async Task EnsureMediaAvailableAsync(Guid mediaId, CancellationToken cancellationToken)
        {
            var exists = await _context.Media.AnyAsync(m => m.Id == mediaId && !m.IsDeleting, cancellationToken);

            if (!exists)
            {
                throw new ArgumentException("تصویر انتخاب‌شده یافت نشد یا در حال حذف است.");
            }
        }

        private async Task EnsureParentValidAsync(int? categoryId, int parentId, CancellationToken cancellationToken)
        {
            var tree = await LoadTreeAsync(cancellationToken);

            if (!tree.Contains(parentId))
            {
                throw new ArgumentException("دسته‌بندی والد انتخاب‌شده یافت نشد.");
            }

            var height = 1;

            if (categoryId.HasValue)
            {
                if (parentId == categoryId.Value)
                {
                    throw new ArgumentException("یک دسته‌بندی نمی‌تواند والد خودش باشد.");
                }

                if (tree.GetDescendantIds(categoryId.Value).Contains(parentId))
                {
                    throw new ArgumentException("دسته‌بندی والد نمی‌تواند یکی از زیرمجموعه‌های همین دسته‌بندی باشد.");
                }

                height = tree.GetHeight(categoryId.Value);
            }

            if (tree.GetDepth(parentId) + height > MaxDepth)
            {
                throw new ArgumentException($"عمق درخت دسته‌بندی نمی‌تواند بیشتر از {MaxDepth} سطح باشد.");
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
                throw new ConflictException("این دسته‌بندی هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این نشانی (Slug) هم‌زمان توسط دسته‌بندی دیگری ثبت شد. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(foreignKeyConflictMessage);
            }
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var name = dto.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                throw new ArgumentException("نام دسته‌بندی الزامی است.");
            }

            if (dto.ImageMediaId.HasValue)
            {
                await EnsureMediaAvailableAsync(dto.ImageMediaId.Value, cancellationToken);
            }

            if (dto.ParentCategoryId.HasValue)
            {
                await EnsureParentValidAsync(null, dto.ParentCategoryId.Value, cancellationToken);
            }

            var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, name, candidate => _context.Categories.AnyAsync(c => c.Slug == candidate, cancellationToken), SlugMaxLength);

            var category = new Category
            {
                Name = name,
                Slug = slug,
                Description = NormalizeDescription(dto.Description),
                ImageMediaId = dto.ImageMediaId,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);

            await SaveAsync(ReferenceDeletedMessage, cancellationToken);

            return Map(category, await GetBreadcrumbAsync(category.Id, cancellationToken));
        }

        public async Task<PagedResult<CategoryListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
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

            var query = _context.Categories.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(c => c.Name.Contains(term) || c.Slug.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderBy(c => c.Name).ThenBy(c => c.Id).Skip((int)skip).Take(pageSize).Select(c => new CategoryListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    ImageMediaId = c.ImageMediaId,
                    IsActive = c.IsActive,
                    ChildCount = c.ChildCategories.Count(),
                    DirectProductCount = c.Products.Count()
                }).ToListAsync(cancellationToken);

            return new PagedResult<CategoryListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Id).Select(c => new
                {
                    c.ParentCategoryId,
                    Node = new CategoryTreeDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        ImageMediaId = c.ImageMediaId,
                        IsActive = c.IsActive
                    }
                }).ToListAsync(cancellationToken);

            var nodes = rows.ToDictionary(r => r.Node.Id, r => r.Node);

            var roots = new List<CategoryTreeDto>();

            foreach (var row in rows)
            {
                if (row.ParentCategoryId.HasValue && nodes.TryGetValue(row.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(row.Node);
                }
                else
                {
                    roots.Add(row.Node);
                }
            }

            return roots;
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (category == null)
            {
                return null;
            }

            return Map(category, await GetBreadcrumbAsync(id, cancellationToken));
        }

        public async Task<CategoryDto> EditAsync(int id, EditCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

            if (dto.ImageMediaId.HasValue && dto.RemoveImage)
            {
                throw new ArgumentException("نمی‌توان هم‌زمان تصویر جدید تعیین کرد و تصویر فعلی را حذف کرد.");
            }

            if (dto.ParentCategoryId.HasValue && dto.RemoveParent)
            {
                throw new ArgumentException("نمی‌توان هم‌زمان والد جدید تعیین کرد و دسته‌بندی را به دسته‌ی اصلی تبدیل کرد.");
            }

            var changed = false;

            if (dto.Name != null)
            {
                var name = dto.Name.Trim();

                if (name.Length == 0)
                {
                    throw new ArgumentException("نام دسته‌بندی نمی‌تواند خالی باشد.");
                }

                if (name != category.Name)
                {
                    category.Name = name;

                    changed = true;
                }
            }

            if (dto.Slug != null)
            {
                var slug = await SlugHelper.GenerateUniqueAsync(dto.Slug, category.Name, candidate => _context.Categories.AnyAsync(c => c.Slug == candidate && c.Id != category.Id, cancellationToken), SlugMaxLength);

                if (slug != category.Slug)
                {
                    category.Slug = slug;

                    changed = true;
                }
            }

            if (dto.Description != null)
            {
                var description = NormalizeDescription(dto.Description);

                if (description != category.Description)
                {
                    category.Description = description;

                    changed = true;
                }
            }

            if (dto.RemoveImage && category.ImageMediaId != null)
            {
                category.ImageMediaId = null;

                changed = true;
            }

            if (dto.ImageMediaId.HasValue && dto.ImageMediaId != category.ImageMediaId)
            {
                await EnsureMediaAvailableAsync(dto.ImageMediaId.Value, cancellationToken);

                category.ImageMediaId = dto.ImageMediaId;

                changed = true;
            }

            if (dto.RemoveParent && category.ParentCategoryId != null)
            {
                category.ParentCategoryId = null;

                changed = true;
            }

            if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId != category.ParentCategoryId)
            {
                await EnsureParentValidAsync(category.Id, dto.ParentCategoryId.Value, cancellationToken);

                category.ParentCategoryId = dto.ParentCategoryId;

                changed = true;
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != category.IsActive)
            {
                category.IsActive = dto.IsActive.Value;

                changed = true;
            }

            if (changed)
            {
                category.UpdatedAt = DateTime.UtcNow;

                await SaveAsync(ReferenceDeletedMessage, cancellationToken);
            }

            return Map(category, await GetBreadcrumbAsync(category.Id, cancellationToken));
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ?? throw new NotFoundException("دسته‌بندی مورد نظر یافت نشد.");

            var hasProducts = await _context.Categories.AnyAsync(c => c.Id == id && c.Products.Any(), cancellationToken);

            if (hasProducts)
            {
                throw new ConflictException(HasProductsMessage);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var now = DateTime.UtcNow;

            await _context.Categories.Where(c => c.ParentCategoryId == id).ExecuteUpdateAsync(setters => setters.SetProperty(c => c.ParentCategoryId, (int?)null).SetProperty(c => c.UpdatedAt, (DateTime?)now), cancellationToken);

            _context.Categories.Remove(category);

            await SaveAsync(DeleteConflictMessage, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}