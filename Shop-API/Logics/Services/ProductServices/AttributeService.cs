using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_API.Data;
using Shop_API.DTOs.AttributeDTOs;
using Shop_API.DTOs.MediaDTOs;
using Shop_API.Exceptions;
using Shop_API.Logics.IServices.IProductServices;
using Shop_API.Models.ProductModels;

namespace Shop_API.Logics.Services.ProductServices
{
    public class AttributeService : IAttributeService
    {
        private const int MaxPageSize = 100;
        private const int CodeMaxLength = 100;
        private const string InUseMessage = "این ویژگی در حداقل یک نوع محصول استفاده شده و قابل حذف نیست. در صورت نیاز می‌توانید آن را غیرفعال کنید.";

        private readonly AppDbContext _context;

        public AttributeService(AppDbContext context)
        {
            _context = context;
        }

        private static AttributeDto Map(ProductAttribute attribute)
        {
            return new AttributeDto
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Code = attribute.Code,
                IsColor = attribute.IsColor,
                IsActive = attribute.IsActive,
                CreatedAt = attribute.CreatedAt,
                UpdatedAt = attribute.UpdatedAt
            };
        }

        private static bool IsValidCode(string code)
        {
            if (code.Length == 0 || code.Length > CodeMaxLength || !char.IsAsciiLetterLower(code[0]))
            {
                return false;
            }

            return code.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '_');
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("این ویژگی هم‌زمان توسط عملیات دیگری تغییر کرده است. لطفاً دوباره تلاش کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("این کد ویژگی هم‌زمان توسط ویژگی دیگری ثبت شد. لطفاً کد دیگری انتخاب کنید.");
            }
            catch (DbUpdateException exception) when (exception.GetBaseException() is SqlException { Number: 547 })
            {
                throw new ConflictException(InUseMessage);
            }
        }

        public async Task<AttributeDto> CreateAsync(CreateAttributeDto dto, CancellationToken cancellationToken = default)
        {
            var name = dto.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                throw new ArgumentException("نام ویژگی الزامی است.");
            }

            var code = dto.Code ?? string.Empty;

            if (!IsValidCode(code))
            {
                throw new ArgumentException("کد ویژگی فقط می‌تواند شامل حروف کوچک انگلیسی، عدد و زیرخط باشد و باید با حرف شروع شود.");
            }

            var codeExists = await _context.ProductAttributes.AnyAsync(a => a.Code == code, cancellationToken);

            if (codeExists)
            {
                throw new ConflictException("این کد ویژگی قبلاً ثبت شده است.");
            }

            var attribute = new ProductAttribute
            {
                Name = name,
                Code = code,
                IsColor = dto.IsColor,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductAttributes.Add(attribute);

            await SaveAsync(cancellationToken);

            return Map(attribute);
        }

        public async Task<PagedResult<AttributeListDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
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

            var query = _context.ProductAttributes.AsNoTracking();

            var term = search?.Trim();

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(a => a.Name.Contains(term) || a.Code.Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderBy(a => a.Name).ThenBy(a => a.Id).Skip((int)skip).Take(pageSize).Select(a => new AttributeListDto
            {
                Id = a.Id,
                Name = a.Name,
                Code = a.Code,
                IsColor = a.IsColor,
                IsActive = a.IsActive,
                UsageCount = a.VariantAttributes.Count()
            }).ToListAsync(cancellationToken);

            return new PagedResult<AttributeListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<AttributeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.ProductAttributes.AsNoTracking().Where(a => a.Id == id).Select(a => new AttributeDto
            {
                Id = a.Id,
                Name = a.Name,
                Code = a.Code,
                IsColor = a.IsColor,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AttributeDto> EditAsync(int id, EditAttributeDto dto, CancellationToken cancellationToken = default)
        {
            var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

            var changed = false;

            if (dto.Name != null)
            {
                var name = dto.Name.Trim();

                if (name.Length == 0)
                {
                    throw new ArgumentException("نام ویژگی نمی‌تواند خالی باشد.");
                }

                if (name != attribute.Name)
                {
                    attribute.Name = name;

                    changed = true;
                }
            }

            if (dto.IsActive.HasValue && dto.IsActive.Value != attribute.IsActive)
            {
                attribute.IsActive = dto.IsActive.Value;

                changed = true;
            }

            if (changed)
            {
                attribute.UpdatedAt = DateTime.UtcNow;

                await SaveAsync(cancellationToken);
            }

            return Map(attribute);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw new NotFoundException("ویژگی مورد نظر یافت نشد.");

            var isUsed = await _context.VariantAttributes.AnyAsync(v => v.ProductAttributeId == id, cancellationToken);

            if (isUsed)
            {
                throw new ConflictException(InUseMessage);
            }

            _context.ProductAttributes.Remove(attribute);

            await SaveAsync(cancellationToken);
        }
    }
}